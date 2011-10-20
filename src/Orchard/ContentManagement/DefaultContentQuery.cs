using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Impl;
using NHibernate.Linq;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.Utility.Extensions;

namespace Orchard.ContentManagement {
    public class DefaultContentQuery : IContentQuery {
        private readonly ISessionLocator _sessionLocator;
        private ISession _session;
        private ICriteria _itemVersionCriteria;
        private VersionOptions _versionOptions;

        public DefaultContentQuery(IContentManager contentManager, ISessionLocator sessionLocator) {
            _sessionLocator = sessionLocator;
            ContentManager = contentManager;
        }

        public IContentManager ContentManager { get; private set; }

        ISession BindSession() {
            if (_session == null)
                _session = _sessionLocator.For(typeof(ContentItemVersionRecord));
            return _session;
        }

        internal ICriteria BindCriteriaByPath(ICriteria criteria, string path) {
            return criteria.GetCriteriaByPath(path) ?? criteria.CreateCriteria(path);
        }

        internal ICriteria BindTypeCriteria() {
            // ([ContentItemVersionRecord] >join> [ContentItemRecord]) >join> [ContentType]

            return BindCriteriaByPath(BindItemCriteria(), "ContentType");
        }

        internal ICriteria BindItemCriteria() {
            // [ContentItemVersionRecord] >join> [ContentItemRecord]

            return BindCriteriaByPath(BindItemVersionCriteria(), "ContentItemRecord");
        }

        internal ICriteria BindItemVersionCriteria() {
            if (_itemVersionCriteria == null) {
                _itemVersionCriteria = BindSession().CreateCriteria<ContentItemVersionRecord>();
            }
            return _itemVersionCriteria;
        }

        internal ICriteria BindPartCriteria<TRecord>() where TRecord : ContentPartRecord {
            if (typeof(TRecord).IsSubclassOf(typeof(ContentPartVersionRecord))) {
                return BindCriteriaByPath(BindItemVersionCriteria(), typeof(TRecord).Name);
            }
            return BindCriteriaByPath(BindItemCriteria(), typeof(TRecord).Name);
        }

        private void ForType(params string[] contentTypeNames) {
            if (contentTypeNames != null && contentTypeNames.Length != 0)
                BindTypeCriteria().Add(Restrictions.InG("Name", contentTypeNames));
        }

        public void ForVersion(VersionOptions options) {
            _versionOptions = options;
        }

        private void Where<TRecord>() where TRecord : ContentPartRecord {
            // this simply demands an inner join
            BindPartCriteria<TRecord>();
        }

        private void Where<TRecord>(Expression<Func<TRecord, bool>> predicate) where TRecord : ContentPartRecord {

            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).Where(predicate);

            // translate it into the nhibernate ICriteria implementation
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attach the criterion from the predicate to this query's criteria for the record
            var recordCriteria = BindPartCriteria<TRecord>();
            foreach (var expressionEntry in criteria.IterateExpressionEntries()) {
                recordCriteria.Add(expressionEntry.Criterion);
            }
        }

        private void Where(Action<IExpressionFactory> expression) {
            var expressionFactory = new DefaultExpressionFactory(this);

            expression(expressionFactory);
            if (expressionFactory.Criterion != null) {
                expressionFactory.Criteria.Add(expressionFactory.Criterion);
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

        private void OrderBy(Action<ISortFactory> expression) {
            var sortFactory = new DefaultSortFactory(this);

            expression(sortFactory);
            if (sortFactory.Order != null) {
                sortFactory.Criteria.AddOrder(sortFactory.Order);
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

            // TODO: put 'removed false' filter in place
            if (skip != 0) {
                criteria = criteria.SetFirstResult(skip);
            }
            if (count != 0) {
                criteria = criteria.SetMaxResults(count);
            }
            return criteria
                .List<ContentItemVersionRecord>()
                .Select(x => ContentManager.Get(x.Id, VersionOptions.VersionRecord(x.Id)))
                .ToReadOnlyCollection();
        }

        int Count() {
            var criteria = (ICriteria)BindItemVersionCriteria().Clone();
            criteria.ClearOrders();

            criteria.ApplyVersionOptionsRestrictions(_versionOptions);

            return criteria.SetProjection(Projections.RowCount()).UniqueResult<Int32>();
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

            IContentQuery<T> IContentQuery<T>.Where(Action<IExpressionFactory> predicate) {
                _query.Where(predicate);
                return new ContentQuery<T>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.Where<TRecord>(Expression<Func<TRecord, bool>> predicate) {
                _query.Where(predicate);
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
                _query.OrderBy(keySelector);
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T, TRecord> IContentQuery<T>.OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
                _query.OrderByDescending(keySelector);
                return new ContentQuery<T, TRecord>(_query);
            }

            IContentQuery<T> IContentQuery<T>.OrderBy(Action<ISortFactory> order) {
                _query.OrderBy(order);
                return new ContentQuery<T>(_query);
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
        }

        public class DefaultExpressionFactory : IExpressionFactory {
            private readonly DefaultContentQuery _query;
            public ICriterion Criterion { get; private set; }
            public ICriteria Criteria { get; private set; }

            public DefaultExpressionFactory(DefaultContentQuery query) {
                _query = query;
            }

            public IExpressionFactory WithRecord(string path) {
                Criteria = _query.BindCriteriaByPath(_query.BindItemCriteria(), path);
                return this;
            }

            public IExpressionFactory WithRelationship(string path) {
                Criteria = _query.BindCriteriaByPath(Criteria, path);
                return this;
            }

            public IExpressionFactory WithVersionRecord(string path) {
                Criteria = _query.BindCriteriaByPath(_query.BindItemVersionCriteria(), path);
                return this;
            }

            public void Eq(string propertyName, object value) {
                Criterion = Restrictions.Eq(propertyName, value);
            }

            public void Like(string propertyName, object value) {
                Criterion = Restrictions.Like(propertyName, value);
            }

            public void Like(string propertyName, string value, MatchMode matchMode, char? escapeChar) {
                Criterion = Restrictions.Like(propertyName, value, ToMatchMode(matchMode), escapeChar);
            }

            public void Like(string propertyName, string value, MatchMode matchMode) {
                Criterion = Restrictions.Like(propertyName, value, ToMatchMode(matchMode));
            }

            public void InsensitiveLike(string propertyName, string value, MatchMode matchMode) {
                Criterion = Restrictions.InsensitiveLike(propertyName, value, ToMatchMode(matchMode));
            }

            public void InsensitiveLike(string propertyName, object value) {
                Criterion = Restrictions.InsensitiveLike(propertyName, value);
            }

            public void Gt(string propertyName, object value) {
                Criterion = Restrictions.Gt(propertyName, value);
            }

            public void Lt(string propertyName, object value) {
                Criterion = Restrictions.Lt(propertyName, value);
            }

            public void Le(string propertyName, object value) {
                Criterion = Restrictions.Le(propertyName, value);
            }

            public void Ge(string propertyName, object value) {
                Criterion = Restrictions.Ge(propertyName, value);
            }

            public void Between(string propertyName, object lo, object hi) {
                Criterion = Restrictions.Between(propertyName, lo, hi);
            }

            public void In(string propertyName, object[] values) {
                Criterion = Restrictions.In(propertyName, values);
            }

            public void In(string propertyName, ICollection values) {
                Criterion = Restrictions.In(propertyName, values);
            }

            public void InG<T>(string propertyName, ICollection<T> values) {
                Criterion = Restrictions.InG(propertyName, values);
            }

            public void IsNull(string propertyName) {
                Criterion = Restrictions.IsNull(propertyName);
            }

            public void EqProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.EqProperty(propertyName, otherPropertyName);
            }

            public void NotEqProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.NotEqProperty(propertyName, otherPropertyName);
            }

            public void GtProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.GtProperty(propertyName, otherPropertyName);
            }

            public void GeProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.GeProperty(propertyName, otherPropertyName);
            }

            public void LtProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.LtProperty(propertyName, otherPropertyName);
            }

            public void LeProperty(string propertyName, string otherPropertyName) {
                Criterion = Restrictions.LeProperty(propertyName, otherPropertyName);
            }

            public void IsNotNull(string propertyName) {
                Criterion = Restrictions.IsNotNull(propertyName);
            }

            public void IsNotEmpty(string propertyName) {
                Criterion = Restrictions.IsNotEmpty(propertyName);
            }

            public void IsEmpty(string propertyName) {
                Criterion = Restrictions.IsEmpty(propertyName);
            }

            public void And(Action<IExpressionFactory> lhs, Action<IExpressionFactory> rhs) {
                lhs(this);
                var a = Criterion;
                rhs(this);
                var b = Criterion;
                Criterion = Restrictions.And(a, b);
            }

            public void Or(Action<IExpressionFactory> lhs, Action<IExpressionFactory> rhs) {
                lhs(this);
                var a = Criterion;
                rhs(this);
                var b = Criterion;
                Criterion = Restrictions.Or(a, b);
            }

            public void Not(Action<IExpressionFactory> expression) {
                expression(this);
                var a = Criterion;
                Criterion = Restrictions.Not(a);
            }

            public void Conjunction(Action<IExpressionFactory> expression, params Action<IExpressionFactory>[] otherExpressions) {
                var junction = Restrictions.Conjunction();
                foreach (var exp in Enumerable.Empty<Action<IExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                    exp(this);
                    junction.Add(Criterion);
                }

                Criterion = junction;
            }

            public void Disjunction(Action<IExpressionFactory> expression, params Action<IExpressionFactory>[] otherExpressions) {
                var junction = Restrictions.Disjunction();
                foreach (var exp in Enumerable.Empty<Action<IExpressionFactory>>().Union(new[] { expression }).Union(otherExpressions)) {
                    exp(this);
                    junction.Add(Criterion);
                }

                Criterion = junction;
            }

            public void AllEq(IDictionary propertyNameValues) {
                Criterion = Restrictions.AllEq(propertyNameValues);
            }

            public void NaturalId() {
                Criterion = Restrictions.NaturalId();
            }

            private static NHibernate.Criterion.MatchMode ToMatchMode(MatchMode matchMode) {
                switch(matchMode) {
                    case MatchMode.Anywhere:
                        return NHibernate.Criterion.MatchMode.Anywhere;
                    case MatchMode.End:
                        return NHibernate.Criterion.MatchMode.End;
                    case MatchMode.Start:
                        return NHibernate.Criterion.MatchMode.Start;
                    case MatchMode.Exact:
                        return NHibernate.Criterion.MatchMode.Exact;
                }

                return NHibernate.Criterion.MatchMode.Anywhere;
            }
        }

        public class DefaultSortFactory : ISortFactory {
            private readonly DefaultContentQuery _query;
            public Order Order { get; private set; }
            public ICriteria Criteria { get; private set; }

            public DefaultSortFactory(DefaultContentQuery query) {
                _query = query;
            }

            public ISortFactory WithRecord(string path) {
                Criteria = _query.BindCriteriaByPath(_query.BindItemCriteria(), path);
                return this;
            }

            public ISortFactory WithVersionRecord(string path) {
                Criteria = _query.BindCriteriaByPath(_query.BindItemVersionCriteria(), path);
                return this;
            }

            public ISortFactory WithRelationship(string path) {
                Criteria = _query.BindCriteriaByPath(Criteria, path);
                return this;
            }

            public void Asc(string propertyName) {
                Order = Order.Asc(propertyName);
            }

            public void Desc(string propertyName) {
                Order = Order.Desc(propertyName);
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
            else if (versionOptions.IsDraft) {
                criteria.Add(Restrictions.And(
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