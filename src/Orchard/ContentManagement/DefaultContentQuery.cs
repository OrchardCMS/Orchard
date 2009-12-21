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

namespace Orchard.ContentManagement {
    public class DefaultContentQuery : IContentQuery {
        private readonly ISessionLocator _sessionLocator;
        private ISession _session;
        private ICriteria _itemCriteria;

        public DefaultContentQuery(IContentManager contentManager, ISessionLocator sessionLocator) {
            _sessionLocator = sessionLocator;
            ContentManager = contentManager;
        }

        public IContentManager ContentManager { get; private set; }

        ISession BindSession() {
            if (_session == null)
                _session = _sessionLocator.For(typeof(ContentItemRecord));
            return _session;
        }

        ICriteria BindItemCriteria() {
            if (_itemCriteria == null) {
                _itemCriteria = BindSession().CreateCriteria<ContentItemRecord>();
            }
            return _itemCriteria;
        }

        ICriteria BindCriteriaByPath(string path) {
            var itemCriteria = BindItemCriteria();

            // special if the content item is ever used as where or order
            if (path == typeof(ContentItemRecord).Name)
                return itemCriteria;

            return itemCriteria.GetCriteriaByPath(path) ?? itemCriteria.CreateCriteria(path);
        }


        private void ForType(params string[] contentTypeNames) {
            BindCriteriaByPath("ContentType").Add(Restrictions.InG("Name", contentTypeNames));
            return;
        }


        private void Where<TRecord>() {
            // this simply demands an inner join
            BindCriteriaByPath(typeof(TRecord).Name);
            return;
        }

        private void Where<TRecord>(Expression<Func<TRecord, bool>> predicate) {

            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).Where(predicate);

            // translate it into the nhibernate ICriteria implementation
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attach the criterion from the predicate to this query's criteria for the record
            var recordCriteria = BindCriteriaByPath(typeof(TRecord).Name);
            foreach (var expressionEntry in criteria.IterateExpressionEntries()) {
                recordCriteria.Add(expressionEntry.Criterion);
            }

            return;
        }

        private void OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).OrderBy(keySelector);

            // translate it into the nhibernate ordering
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attaching orderings to the query's criteria
            var recordCriteria = BindCriteriaByPath(typeof(TRecord).Name);
            foreach (var ordering in criteria.IterateOrderings()) {
                recordCriteria.AddOrder(ordering.Order);
            }

            return;
        }

        private void OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) {
            // build a linq to nhibernate expression
            var options = new QueryOptions();
            var queryProvider = new NHibernateQueryProvider(BindSession(), options);
            var queryable = new Query<TRecord>(queryProvider, options).OrderByDescending(keySelector);

            // translate it into the nhibernate ICriteria implementation
            var criteria = (CriteriaImpl)queryProvider.TranslateExpression(queryable.Expression);

            // attaching orderings to the query's criteria
            var recordCriteria = BindCriteriaByPath(typeof(TRecord).Name);
            foreach (var ordering in criteria.IterateOrderings()) {
                recordCriteria.AddOrder(ordering.Order);
            }
            return;
        }

        private IEnumerable<ContentItem> List() {
            return BindItemCriteria()
                .List<ContentItemRecord>()
                .Select(x => ContentManager.Get(x.Id));
        }

        private IEnumerable<ContentItem> Slice(int skip, int count) {
            var criteria = BindItemCriteria();
            if (skip != 0)
                criteria = criteria.SetFirstResult(skip);
            if (count != 0)
                criteria = criteria.SetMaxResults(count);
            return criteria
                .List<ContentItemRecord>()
                .Select(x => ContentManager.Get(x.Id));
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

            public IContentQuery<TPart> ForPart<TPart>() where TPart : IContent {
                return new ContentQuery<TPart>(_query);
            }

            public IContentQuery<T> ForType(params string[] contentTypes) {
                _query.ForType(contentTypes);
                return this;
            }

            public IEnumerable<T> List() {
                return _query.List().AsPart<T>();
            }

            public IEnumerable<T> Slice(int skip, int count) {
                return _query.Slice(skip, count).AsPart<T>();
            }

            public IContentQuery<T, TRecord> Join<TRecord>() where TRecord : ContentPartRecord {
                _query.Where<TRecord>();
                return new ContentQuery<T, TRecord>(_query);
            }

            public IContentQuery<T, TRecord> Where<TRecord>(Expression<Func<TRecord, bool>> predicate) where TRecord : ContentPartRecord {
                _query.Where(predicate);
                return new ContentQuery<T, TRecord>(_query);
            }

            public IContentQuery<T, TRecord> OrderBy<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
                _query.OrderBy(keySelector);
                return new ContentQuery<T, TRecord>(_query);
            }

            public IContentQuery<T, TRecord> OrderByDescending<TRecord, TKey>(Expression<Func<TRecord, TKey>> keySelector) where TRecord : ContentPartRecord {
                _query.OrderByDescending(keySelector);
                return new ContentQuery<T, TRecord>(_query);
            }
        }

        class ContentQuery<T, TR> : ContentQuery<T>, IContentQuery<T, TR>
            where T : IContent
            where TR : ContentPartRecord {
            public ContentQuery(DefaultContentQuery query)
                : base(query) {
            }

            public IContentQuery<T, TR> Where(Expression<Func<TR, bool>> predicate) {
                _query.Where(predicate);
                return this;
            }

            public IContentQuery<T, TR> OrderBy<TKey>(Expression<Func<TR, TKey>> keySelector) {
                _query.OrderBy(keySelector);
                return this;
            }

            public IContentQuery<T, TR> OrderByDescending<TKey>(Expression<Func<TR, TKey>> keySelector) {
                _query.OrderByDescending(keySelector);
                return this;
            }
        }

   }
}