using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;

namespace NHibernate.Linq.Util
{
    public static class DetachedCriteriaExtensions
    {
        public static ICriteria Adapt(this DetachedCriteria criteria, ISession session)
        {
            if (criteria == null) return null;
            return new DetachedCriteriaAdapter(criteria, session);
        }
    }

    public class DetachedCriteriaAdapter : ICriteria
    {
        public DetachedCriteriaAdapter(DetachedCriteria detachedCriteria, ISession session)
        {
            this.DetachedCriteria = detachedCriteria;
            this.Session = session;
        }

        public DetachedCriteria DetachedCriteria { get; }

        public ISession Session { get; }

        #region ICriteria Members

        public IProjection Projection => null;
        public ICriteria Add(ICriterion expression)
        {
            return DetachedCriteria.Add(expression).Adapt(Session);
        }

        public ICriteria AddOrder(Order order)
        {
            return DetachedCriteria.AddOrder(order).Adapt(Session);
        }

        public string Alias => DetachedCriteria.Alias;

        public void ClearOrderds()
        {
            throw new NotSupportedException();
        }

        public ICriteria CreateAlias(string associationPath, string alias, JoinType joinType)
        {
            return DetachedCriteria.CreateAlias(associationPath, alias, joinType).Adapt(Session);
        }

        public ICriteria CreateAlias(string associationPath, string alias)
        {
            return DetachedCriteria.CreateAlias(associationPath, alias).Adapt(Session);
        }

        public ICriteria CreateAlias(string associationPath, string alias, JoinType joinType, ICriterion withClause)
        {
            throw new NotImplementedException();
        }

        public ICriteria CreateCriteria(string associationPath, string alias, JoinType joinType)
        {
            return DetachedCriteria.CreateCriteria(associationPath, alias, joinType).Adapt(Session);
        }

        public ICriteria CreateCriteria(string associationPath, string alias)
        {
            return DetachedCriteria.CreateCriteria(associationPath, alias).Adapt(Session);
        }

        public ICriteria CreateCriteria(string associationPath, JoinType joinType)
        {
            return DetachedCriteria.CreateCriteria(associationPath, joinType).Adapt(Session);
        }

        public ICriteria CreateCriteria(string associationPath, string alias, JoinType joinType, ICriterion withClause)
        {
            throw new NotImplementedException();
        }

        public ICriteria CreateCriteria(string associationPath)
        {
            return DetachedCriteria.CreateCriteria(associationPath).Adapt(Session);
        }

        public ICriteria GetCriteriaByAlias(string alias)
        {
            return DetachedCriteria.GetCriteriaByAlias(alias).Adapt(Session);
        }

        public ICriteria GetCriteriaByPath(string path)
        {
            return DetachedCriteria.GetCriteriaByPath(path).Adapt(Session);
        }

        public IList<T> List<T>()
        {
            throw new NotSupportedException();
        }

        public void List(IList results)
        {
            throw new NotSupportedException();
        }

        public IList List()
        {
            throw new NotSupportedException();
        }

        public ICriteria SetCacheMode(CacheMode cacheMode)
        {
            return DetachedCriteria.SetCacheMode(cacheMode).Adapt(Session);
        }

        public ICriteria SetCacheRegion(string cacheRegion)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetCacheable(bool cacheable)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetComment(string comment)
        {
            throw new NotSupportedException();
        }

        [Obsolete("Use Fetch instead")]
        public ICriteria SetFetchMode(string associationPath, FetchMode mode)
        {
            return DetachedCriteria.SetFetchMode(associationPath, mode).Adapt(Session);
        }

        public ICriteria SetFetchSize(int fetchSize)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetFirstResult(int firstResult)
        {
            return DetachedCriteria.SetFirstResult(firstResult).Adapt(Session);
        }

        public ICriteria SetFlushMode(FlushMode flushMode)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetLockMode(string alias, LockMode lockMode)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetLockMode(LockMode lockMode)
        {
            throw new NotSupportedException();
        }

        public ICriteria SetMaxResults(int maxResults)
        {
            return DetachedCriteria.SetMaxResults(maxResults).Adapt(Session);
        }

        public ICriteria SetProjection(IProjection projection)
        {
            return DetachedCriteria.SetProjection(projection).Adapt(Session);
        }

        public ICriteria SetProjection(params IProjection[] projections)
        {
            var projectionList = Projections.ProjectionList();
            foreach (var proj in projections)
                projectionList.Add(proj);

            return DetachedCriteria.SetProjection(projectionList).Adapt(Session);
        }

        public ICriteria SetResultTransformer(IResultTransformer resultTransformer)
        {
            return DetachedCriteria.SetResultTransformer(resultTransformer).Adapt(Session);
        }

        public ICriteria SetTimeout(int timeout)
        {
            throw new NotSupportedException();
        }

        public T UniqueResult<T>()
        {
            throw new NotSupportedException();
        }

        public object UniqueResult()
        {
            throw new NotSupportedException();
        }

        public System.Type GetRootEntityTypeIfAvailable()
        {
            return DetachedCriteria.GetRootEntityTypeIfAvailable();
        }

        public void ClearOrders()
        {
            DetachedCriteria.ClearOrders();
        }

        public IEnumerable<T> Future<T>()
        {
            throw new NotSupportedException();
        }

        public IFutureValue<T> FutureValue<T>()
        {
            throw new NotSupportedException();
        }

        public Task<IList> ListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<object> UniqueResultAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task ListAsync(IList results, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<IList<T>> ListAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        public Task<T> UniqueResultAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotSupportedException();
        }

        IFutureEnumerable<T> ICriteria.Future<T>()
        {
            throw new NotSupportedException();
        }
        #endregion

        #region ICloneable Members

        public object Clone()
        {
            throw new NotSupportedException();
        }

        #endregion



        public bool IsReadOnly { get; private set; }

        public bool IsReadOnlyInitialized { get; private set; }

        public ICriteria SetReadOnly(bool readOnly)
        {
            IsReadOnly = readOnly;
            IsReadOnlyInitialized = true;
            return this;
        }

    }
}