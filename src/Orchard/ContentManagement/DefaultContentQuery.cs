using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Linq;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using NHibernate.Transform;
using NHibernate.SqlCommand;
using Orchard.Utility.Extensions;
using Orchard.Caching;

namespace Orchard.ContentManagement {
    public class DefaultContentQuery : IContentQuery {
        private readonly ITransactionManager _transactionManager;
        private ISession _session;
        private ICriteria _itemVersionCriteria;
        private VersionOptions _versionOptions;
        private ICacheManager _cacheManager;
        private ISignals _signals;
        private IRepository<ContentTypeRecord> _contentTypeRepository;
        private IEnumerable<IGlobalCriteriaProvider> _globalCriteriaList;

        public DefaultContentQuery(
            IContentManager contentManager, 
            ITransactionManager transactionManager, 
            ICacheManager cacheManager,
            ISignals signals,
            IRepository<ContentTypeRecord> contentTypeRepository,
            IEnumerable<IGlobalCriteriaProvider> globalCriteriaList) {
            _transactionManager = transactionManager;
            ContentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            _contentTypeRepository = contentTypeRepository;
            _globalCriteriaList = globalCriteriaList;
        }

        public IContentManager ContentManager { get; private set; }

        private void BeforeExecuteQuery(ICriteria contentItemVersionCriteria) {
            foreach(var criteria in _globalCriteriaList) {
                criteria.AddCriteria(contentItemVersionCriteria);
            }
        }

        ISession BindSession() {
            if (_session == null)
                _session = _transactionManager.GetSession();
            return _session;
        }

        ICriteria BindCriteriaByPath(ICriteria criteria, string path) {
            return criteria.GetCriteriaByPath(path) ?? criteria.CreateCriteria(path);
        }

        //ICriteria BindTypeCriteria() {
        //    // ([ContentItemVersionRecord] >join> [ContentItemRecord]) >join> [ContentType]

        //    return BindCriteriaByPath(BindItemCriteria(), "ContentType");
        //}

        ICriteria BindItemCriteria() {
            // [ContentItemVersionRecord] >join> [ContentItemRecord]

            return BindCriteriaByPath(BindItemVersionCriteria(), "ContentItemRecord");
        }

        ICriteria BindItemVersionCriteria() {
            if (_itemVersionCriteria == null) {
                _itemVersionCriteria = BindSession().CreateCriteria<ContentItemVersionRecord>();
                _itemVersionCriteria.SetCacheable(true);
            }
            return _itemVersionCriteria;
        }

        ICriteria BindPartCriteria<TRecord>() where TRecord : ContentPartRecord {
            if (typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
                return BindCriteriaByPath(BindItemVersionCriteria(), typeof(TRecord).Name);
            }
            return BindCriteriaByPath(BindItemCriteria(), typeof(TRecord).Name);
        }

        private int GetContentTypeRecordId(string contentType) {
            return _cacheManager.Get(contentType + "_Record", true, ctx => {
                ctx.Monitor(_signals.When(contentType + "_Record"));

                var contentTypeRecord = _contentTypeRepository.Get(x => x.Name == contentType);

                if (contentTypeRecord == null) {
                    //TEMP: this is not safe... ContentItem types could be created concurrently?
                    contentTypeRecord = new ContentTypeRecord { Name = contentType };
                    _contentTypeRepository.Create(contentTypeRecord);
                }

                return contentTypeRecord.Id;
            });
        }

        private void ForType(params string[] contentTypeNames) {
            if (contentTypeNames != null) {
                var contentTypeIds = contentTypeNames.Select(GetContentTypeRecordId).ToArray();
                // don't use the IN operator if not needed for performance reasons
                if (contentTypeNames.Length == 1) {
                    BindItemCriteria().Add(Restrictions.Eq("ContentType.Id", contentTypeIds[0]));
                }
                else {
                    BindItemCriteria().Add(Restrictions.InG("ContentType.Id", contentTypeIds));
                }
            }
        }

        public void ForVersion(VersionOptions options) {
            _versionOptions = options;
        }

        private void ForContentItems(IEnumerable<int> ids) {
            if (ids == null) throw new ArgumentNullException("ids");

            // Converting to array as otherwise an exception "Expression argument must be of type ICollection." is thrown.
            Where<ContentItemRecord>(record => ids.ToArray().Contains(record.Id), BindCriteriaByPath(BindItemCriteria(), typeof(ContentItemRecord).Name));
        }

        private void Where<TRecord>() where TRecord : ContentPartRecord {
            // this simply demands an inner join
            BindPartCriteria<TRecord>();
        }

        private void Where<TRecord>(Expression<Func<TRecord, bool>> predicate) where TRecord : ContentPartRecord {
            Where<TRecord>(predicate, BindPartCriteria<TRecord>());
        }

        private void Where<TRecord>(Expression<Func<TRecord, bool>> predicate, ICriteria bindCriteria) {
            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).Where(predicate);

            // translate it into the nhibernate ICriteria implementation
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attach the criterion from the predicate to this query's criteria for the record
            var recordCriteria = bindCriteria;
            foreach (var expressionEntry in criteria.IterateExpressionEntries()) {
                recordCriteria.Add(expressionEntry.Criterion);
            }
        }

        private void OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).OrderBy(keySelector);

            // translate it into the nhibernate ordering
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attaching orderings to the query's criteria
            var recordCriteria = BindPartCriteria<TRecord>();
            foreach (var ordering in criteria.IterateOrderings()) {
                recordCriteria.AddOrder(ordering.Order);
            }
        }

        private void OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).OrderByDescending(keySelector);

            // translate it into the nhibernate ICriteria implementation
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attaching orderings to the query's criteria
            var recordCriteria = BindPartCriteria<TRecord>();
            foreach (var ordering in criteria.IterateOrderings()) {
                recordCriteria.AddOrder(ordering.Order);
            }
        }

        private IEnumerable<ContentItem> Slice(int skip, int count) {
            var criteria = BindItemVersionCriteria();
            
            criteria.ApplyVersionOptionsRestrictions(_versionOptions);

            criteria.SetFetchMode("ContentItemRecord", FetchMode.Eager);
            criteria.SetFetchMode("ContentItemRecord.ContentType", FetchMode.Eager);

            // TODO: put 'removed false' filter in place
            if (skip != 0) {
                criteria = criteria.SetFirstResult(skip);
            }
            if (count != 0) {
                criteria = criteria.SetMaxResults(count);
            }

            BeforeExecuteQuery(criteria);

            return criteria
                .List<ContentItemVersionRecord>()
                .Select(x => ContentManager.Get(x.ContentItemRecord.Id, _versionOptions != null && _versionOptions.IsDraftRequired ? _versionOptions : VersionOptions.VersionRecord(x.Id)))
                .ToReadOnlyCollection();
        }

        int Count() {
            var criteria = (ICriteria)BindItemVersionCriteria().Clone();
            criteria.ClearOrders();

            criteria.ApplyVersionOptionsRestrictions(_versionOptions);
            BeforeExecuteQuery(criteria);

            return criteria.SetProjection(Projections.RowCount()).UniqueResult<Int32>();
        }

        void WithQueryHints(QueryHints hints) {
            if (hints == QueryHints.Empty) {
                return;
            }

            var contentItemVersionCriteria = BindItemVersionCriteria();
            var contentItemCriteria = BindItemCriteria();

            var contentItemMetadata = _session.SessionFactory.GetClassMetadata(typeof(ContentItemRecord));
            var contentItemVersionMetadata = _session.SessionFactory.GetClassMetadata(typeof(ContentItemVersionRecord));

            // break apart and group hints by their first segment
            var hintDictionary = hints.Records
                .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
                .GroupBy(item => item.Segments.FirstOrDefault())
                .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

            // locate hints that match properties in the ContentItemVersionRecord
            foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
                contentItemVersionCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
                hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria, ExtendCriteria);
            }

            // locate hints that match properties in the ContentItemRecord
            foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
                contentItemVersionCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
                hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria, ExtendCriteria);
            }

            if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
                contentItemVersionCriteria.SetResultTransformer(new DistinctRootEntityResultTransformer());
        }

        void WithQueryHintsFor(string contentType) {
            var contentItem = ContentManager.New(contentType);
            var contentPartRecords = new List<string>();
            foreach (var part in contentItem.Parts) {
                var partType = part.GetType().BaseType;
                if (partType.IsGenericType && partType.GetGenericTypeDefinition() == typeof(ContentPart<>)) {
                    var recordType = partType.GetGenericArguments().Single();
                    contentPartRecords.Add(recordType.Name);
                }
            }

            WithQueryHints(new QueryHints().ExpandRecords(contentPartRecords));
        }

        private static ICriteria ExtendCriteria(ICriteria criteria, string segment) {
            return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
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

            IContentQuery<T> IContentQuery<T>.ForContentItems(IEnumerable<int> ids) {
                _query.ForContentItems(ids);
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

            IContentQuery<T> IContentQuery<T>.WithQueryHints(QueryHints hints) {
                _query.WithQueryHints(hints);
                return this;
            }

            IContentQuery<T> IContentQuery<T>.WithQueryHintsFor(string contentType) {
                _query.WithQueryHintsFor(contentType);
                return this;
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

            IContentQuery<T, TR> IContentQuery<T, TR>.WithQueryHints(QueryHints hints) {
                _query.WithQueryHints(hints);
                return this;
            }

            IContentQuery<T, TR> IContentQuery<T, TR>.WithQueryHintsFor(string contentType) {
                _query.WithQueryHintsFor(contentType);
                return this;
            }
        }
    }

    internal static class CriteriaExtensions {
        internal static void ApplyVersionOptionsRestrictions(this ICriteria criteria, VersionOptions versionOptions) {
            if (versionOptions == null) {
                criteria.Add(Restrictions.Eq("Published", true));
            }
            else if (versionOptions.IsPublished) {
                criteria.Add(Restrictions.Eq("Published", true));
            }
            else if (versionOptions.IsLatest) {
                criteria.Add(Restrictions.Eq("Latest", true));
            }
            else if (versionOptions.IsDraft && !versionOptions.IsDraftRequired) {
                criteria.Add(Restrictions.And(
                    Restrictions.Eq("Latest", true),
                    Restrictions.Eq("Published", false)));
            }
            else if (versionOptions.IsDraft || versionOptions.IsDraftRequired) {
                criteria.Add(Restrictions.Eq("Latest", true));
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