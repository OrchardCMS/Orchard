using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using NHibernate;
using NHibernate.Criterion;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using NHibernate.Transform;
using NHibernate.SqlCommand;
using Expression = System.Linq.Expressions.Expression;

namespace Orchard.ContentManagement {
    public class DefaultContentQuery : IContentQuery {
        private readonly ISessionLocator _sessionLocator;
        private ISession _session;
        private IQueryOver<ContentItemVersionRecord, ContentItemVersionRecord> _itemVersionQueryOver;
        private readonly IDictionary<string, object> _joins;
        
        private VersionOptions _versionOptions;

        public DefaultContentQuery(IContentManager contentManager, ISessionLocator sessionLocator) {
            _sessionLocator = sessionLocator;
            ContentManager = contentManager;

            _joins = new Dictionary<string, object>();
        }

        public IContentManager ContentManager { get; private set; }

        ISession BindSession() {
            if (_session == null)
                _session = _sessionLocator.For(typeof(ContentItemVersionRecord));
            return _session;
        }

        IQueryOver<ContentItemVersionRecord, TRecord> BindQueryOverByPath<TRecord, TU>(IQueryOver<ContentItemVersionRecord, TU> queryOver, string name) {
            if (_joins.ContainsKey(typeof(TRecord).Name)) {
                return (IQueryOver<ContentItemVersionRecord, TRecord>)_joins[typeof(TRecord).Name];
            }

            // public TPartRecord TPartRecord {get;set;}
            var dynamicMethod = new DynamicMethod(name, typeof(TRecord), null, typeof(TU));
            var syntheticMethod = new ContentItemAlteration.SyntheticMethodInfo(dynamicMethod, typeof(TU));
            var syntheticProperty = new ContentItemAlteration.SyntheticPropertyInfo(syntheticMethod);

            // record => record.TPartRecord
            var parameter = Expression.Parameter(typeof(TU), "record");
            var syntheticExpression = (Expression<Func<TU, TRecord>>)Expression.Lambda(
                typeof(Func<TU, TRecord>),
                Expression.Property(parameter, syntheticProperty),
                parameter);

            var join = queryOver.JoinQueryOver(syntheticExpression);
            _joins[typeof(TRecord).Name] = join;

            return join;
        }

        IQueryOver<ContentItemVersionRecord, ContentTypeRecord> BindTypeQueryOver() {
            // ([ContentItemVersionRecord] >join> [ContentItemRecord]) >join> [ContentType]

            return BindQueryOverByPath<ContentTypeRecord, ContentItemRecord>(BindItemQueryOver(), "ContentType");
        }

        IQueryOver<ContentItemVersionRecord, ContentItemRecord> BindItemQueryOver() {
            // [ContentItemVersionRecord] >join> [ContentItemRecord]

            return BindQueryOverByPath<ContentItemRecord, ContentItemVersionRecord>(BindItemVersionQueryOver(), "ContentItemRecord");
        }

        IQueryOver<ContentItemVersionRecord, ContentItemVersionRecord> BindItemVersionQueryOver() {
            if (_itemVersionQueryOver == null) {
                _itemVersionQueryOver = BindSession().QueryOver<ContentItemVersionRecord>();
                _itemVersionQueryOver.Cacheable();
            }
            return _itemVersionQueryOver;
        }

        IQueryOver<ContentItemVersionRecord, TRecord> BindPartQueryOver<TRecord>() where TRecord : ContentPartRecord {
            if (typeof (TRecord).IsSubclassOf(typeof (ContentPartVersionRecord))) {
                return BindQueryOverByPath<TRecord, ContentItemVersionRecord>(BindItemVersionQueryOver(), typeof(TRecord).Name);
            }

            return BindQueryOverByPath<TRecord, ContentItemRecord>(BindItemQueryOver(), typeof(TRecord).Name);
        }


        private void ForType(params string[] contentTypeNames) {
            if (contentTypeNames != null && contentTypeNames.Length != 0)
                BindTypeQueryOver().Where(Restrictions.InG("Name", contentTypeNames));
        }

        public void ForVersion(VersionOptions options) {
            _versionOptions = options;
        }

        private void Where<TRecord>() where TRecord : ContentPartRecord {
            // this simply demands an inner join
            BindPartQueryOver<TRecord>();
        }

        private void Where<TRecord>(Expression<Func<TRecord, bool>> predicate) where TRecord : ContentPartRecord {
            BindPartQueryOver<TRecord>().Where(predicate);
        }

        private void OrderBy<TRecord>(Expression<Func<TRecord, object>> keySelector) where TRecord : ContentPartRecord {
            BindPartQueryOver<TRecord>().OrderBy(keySelector).Asc();
        }

        private void OrderByDescending<TRecord>(Expression<Func<TRecord, object>> keySelector) where TRecord : ContentPartRecord {
            BindPartQueryOver<TRecord>().OrderBy(keySelector).Desc();
        }

        private void OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
            BindPartQueryOver<TRecord>().OrderBy(AddBox(keySelector)).Asc();
        }

        private void OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
            BindPartQueryOver<TRecord>().OrderBy(AddBox(keySelector)).Desc();
        }

        private static Expression<Func<TInput, object>> AddBox<TInput, TOutput>
        (Expression<Func<TInput, TOutput>> expression) {
            // Add the boxing operation, but get a weakly typed expression
            Expression converted = Expression.Convert
                 (expression.Body, typeof(object));
            // Use Expression.Lambda to get back to strong typing
            return Expression.Lambda<Func<TInput, object>>
                 (converted, expression.Parameters);
        }

        private IEnumerable<ContentItem> Slice(int skip, int count) {
            var queryOver = BindItemVersionQueryOver();

            queryOver.ApplyVersionOptionsRestrictions(_versionOptions);

            // TODO: put 'removed false' filter in place
            if (skip != 0) {
                queryOver.Skip(skip);
            }

            if (count != 0) {
                queryOver.Take(count);
            }

            return new ReadOnlyCollection<ContentItem>(queryOver
                    .List<ContentItemVersionRecord>()
                    .Select(x => ContentManager.Get(x.Id, VersionOptions.VersionRecord(x.Id)))
                    .ToList());
        }

        int Count() {
            var queryOver = BindItemVersionQueryOver();

            // clone the query so that it doesn't affect the current one
            var countQuery = queryOver.Clone();
            countQuery.ClearOrders();

            countQuery.ApplyVersionOptionsRestrictions(_versionOptions);

            return countQuery.Select(Projections.RowCount()).FutureValue<int>().Value;
        }

        IContentQuery<TPart> IContentQuery.ForPart<TPart>() {
            return new ContentQuery<TPart>(this);
        }

        class ContentQuery<T> : IContentQuery<T> where T : IContent {
            protected readonly DefaultContentQuery _query;

            public ContentQuery(DefaultContentQuery query) {
                _query = query;
            }

            public IContentManager ContentManager {
                get { return _query.ContentManager; }
            }

            IContentQuery<TPart> IContentQuery.ForPart<TPart>() {
                return new ContentQuery<TPart>(_query);
            }

            IContentQuery<T> IContentQuery<T>.ForType(params string[] contentTypes) {
                _query.ForType(contentTypes);
                return this;
            }

            IContentQuery<T> IContentQuery<T>.ForVersion(VersionOptions options) {
                _query.ForVersion(options);
                return this;
            }

            IEnumerable<T> IContentQuery<T>.List() {
                return _query.Slice(0, 0).AsPart<T>();
            }

            IEnumerable<T> IContentQuery<T>.Slice(int skip, int count) {
                return _query.Slice(skip, count).AsPart<T>();
            }

            int IContentQuery<T>.Count() {
                return _query.Count();
            }

            IContentQuery<T, TRecord> IContentQuery<T>.Join<TRecord>() {
                _query.Where<TRecord>();
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.Where<TRecord>(Expression<Func<TRecord, bool>> predicate) {
                _query.Where(predicate);
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderBy<TRecord>(Expression<Func<TRecord, object>> keySelector) {
                _query.OrderBy(keySelector);
                 return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderByDescending<TRecord>(Expression<Func<TRecord, object>> keySelector) {
                _query.OrderByDescending(keySelector);
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
                _query.OrderBy(AddBox(keySelector));
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
                _query.OrderByDescending(AddBox(keySelector));
                return new ContentQuery<T, TRecord>(_query);
            }
        }


        class ContentQuery<T, TR> : ContentQuery<T>, IContentQuery<T, TR>
            where T : IContent
            where TR : ContentPartRecord {
            public ContentQuery(DefaultContentQuery query)
                : base(query) {
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.ForVersion(VersionOptions options) {
                _query.ForVersion(options);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.Where(Expression<Func<TR, bool>> predicate) {
                _query.Where(predicate);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.OrderBy<TKey>(Expression<Func<TR, TKey>> keySelector) {
                _query.OrderBy(keySelector);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.OrderByDescending<TKey>(Expression<Func<TR, TKey>> keySelector) {
                _query.OrderByDescending(keySelector);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.OrderBy(Expression<Func<TR, object>> keySelector) {
                _query.OrderBy(keySelector);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.OrderByDescending(Expression<Func<TR, object>> keySelector) {
                _query.OrderByDescending(keySelector);
                return this;
            }

            public IContentQuery<T, TR> WithQueryHints(QueryHints hints) {
                if (hints == QueryHints.Empty) {
                    return this;
                }

                var contentItemVersionCriteria = _query.BindItemVersionQueryOver();
                var contentItemCriteria = _query.BindItemQueryOver();

                var contentItemMetadata = _query._session.SessionFactory.GetClassMetadata(typeof(ContentItemRecord));
                var contentItemVersionMetadata = _query._session.SessionFactory.GetClassMetadata(typeof(ContentItemVersionRecord));

                // break apart and group hints by their first segment
                var hintDictionary = hints.Records
                    .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
                    .GroupBy(item => item.Segments.FirstOrDefault())
                    .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

                // locate hints that match properties in the ContentItemVersionRecord
                foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
                    contentItemVersionCriteria.UnderlyingCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria.UnderlyingCriteria, ExtendCriteria);
                }

                // locate hints that match properties in the ContentItemRecord
                foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
                    contentItemVersionCriteria.UnderlyingCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria.UnderlyingCriteria, ExtendCriteria);
                }

                // if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
                    contentItemVersionCriteria.TransformUsing(new DistinctRootEntityResultTransformer());

                return this;
            }

            private static ICriteria ExtendCriteria(ICriteria criteria, string segment) {
                return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
            }

            public IContentQuery<T, TR> WithQueryHintsFor(string contentType) {
                var contentItem = _query.ContentManager.New(contentType);
                var contentPartRecords = new List<string>();
                foreach (var part in contentItem.Parts) {
                    var partType = part.GetType().BaseType;
                    if (partType.IsGenericType && partType.GetGenericTypeDefinition() == typeof(ContentPart<>)) {
                        var recordType = partType.GetGenericArguments().Single();
                        contentPartRecords.Add(recordType.Name);
                    }
                }

                return WithQueryHints(new QueryHints().ExpandRecords(contentPartRecords));
            }
        }
    }

    internal static class CriteriaExtensions {
        internal static void ApplyVersionOptionsRestrictions<T>(this IQueryOver<T, T> criteria, VersionOptions versionOptions) {
            if (versionOptions == null) {
                criteria.Where(Restrictions.Eq("Published", true));
            }
            else if (versionOptions.IsPublished) {
                criteria.Where(Restrictions.Eq("Published", true));
            }
            else if (versionOptions.IsLatest) {
                criteria.Where(Restrictions.Eq("Latest", true));
            }
            else if (versionOptions.IsDraft) {
                criteria.Where(Restrictions.And(
                    Restrictions.Eq("Latest", true),
                    Restrictions.Eq("Published", false)));
            }
            else if (versionOptions.IsAllVersions) {
                // no-op... all versions will be returned by default
            }
            else {
                throw new ApplicationException("Invalid VersionOptions for content query");
            }
        }
    }
}