using System;
using System.Collections;
using System.Collections.Generic;
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
		private readonly DetachedCriteria detachedCriteria;
		private readonly ISession session;

		public DetachedCriteriaAdapter(DetachedCriteria detachedCriteria, ISession session)
		{
			this.detachedCriteria = detachedCriteria;
			this.session = session;
		}

		public DetachedCriteria DetachedCriteria
		{
			get { return detachedCriteria; }
		}

		public ISession Session
		{
			get { return session; }
		}

		#region ICriteria Members

		public IProjection Projection
		{
			get
			{
				return null;
			}
		}
		public ICriteria Add(ICriterion expression)
		{
			return detachedCriteria.Add(expression).Adapt(session);
		}

		public ICriteria AddOrder(Order order)
		{
			return detachedCriteria.AddOrder(order).Adapt(session);
		}

		public string Alias
		{
			get { return detachedCriteria.Alias; }
		}

		public void ClearOrderds()
		{
			throw new NotSupportedException();
		}

		public ICriteria CreateAlias(string associationPath, string alias, JoinType joinType)
		{
			return detachedCriteria.CreateAlias(associationPath, alias, joinType).Adapt(session);
		}

		public ICriteria CreateAlias(string associationPath, string alias)
		{
			return detachedCriteria.CreateAlias(associationPath, alias).Adapt(session);
		}

        public ICriteria CreateAlias(string associationPath, string alias, JoinType joinType, ICriterion withClause)
        {
            throw new NotImplementedException();
        }

		public ICriteria CreateCriteria(string associationPath, string alias, JoinType joinType)
		{
			return detachedCriteria.CreateCriteria(associationPath, alias, joinType).Adapt(session);
		}

		public ICriteria CreateCriteria(string associationPath, string alias)
		{
			return detachedCriteria.CreateCriteria(associationPath, alias).Adapt(session);
		}

		public ICriteria CreateCriteria(string associationPath, JoinType joinType)
		{
			return detachedCriteria.CreateCriteria(associationPath, joinType).Adapt(session);
		}

        public ICriteria CreateCriteria(string associationPath, string alias, JoinType joinType, ICriterion withClause)
        {
            throw new NotImplementedException();
        }
        
        public ICriteria CreateCriteria(string associationPath)
		{
			return detachedCriteria.CreateCriteria(associationPath).Adapt(session);
		}

		public ICriteria GetCriteriaByAlias(string alias)
		{
			return detachedCriteria.GetCriteriaByAlias(alias).Adapt(session);
		}

		public ICriteria GetCriteriaByPath(string path)
		{
			return detachedCriteria.GetCriteriaByPath(path).Adapt(session);
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
			return detachedCriteria.SetCacheMode(cacheMode).Adapt(session);
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

		public ICriteria SetFetchMode(string associationPath, FetchMode mode)
		{
			return detachedCriteria.SetFetchMode(associationPath, mode).Adapt(session);
		}

		public ICriteria SetFetchSize(int fetchSize)
		{
			throw new NotSupportedException();
		}

		public ICriteria SetFirstResult(int firstResult)
		{
			return detachedCriteria.SetFirstResult(firstResult).Adapt(session);
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
			return detachedCriteria.SetMaxResults(maxResults).Adapt(session);
		}

		public ICriteria SetProjection(IProjection projection)
		{
			return detachedCriteria.SetProjection(projection).Adapt(session);
		}

		public ICriteria SetProjection(params IProjection[] projections)
		{
			var projectionList = Projections.ProjectionList();
			foreach (var proj in projections)
				projectionList.Add(proj);

			return detachedCriteria.SetProjection(projectionList).Adapt(session);
		}

		public ICriteria SetResultTransformer(IResultTransformer resultTransformer)
		{
			return detachedCriteria.SetResultTransformer(resultTransformer).Adapt(session);
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
			return detachedCriteria.GetRootEntityTypeIfAvailable();
		}

		public void ClearOrders()
		{
			detachedCriteria.ClearOrders();
		}

		public IEnumerable<T> Future<T>()
		{
			throw new NotSupportedException();
		}

		public IFutureValue<T> FutureValue<T>()
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


        private bool _readOnly;
        private bool _readOnlyInitialized;

        public bool IsReadOnly
        {
            get
            {
                return _readOnly;
            }
        }

        public bool IsReadOnlyInitialized
        {
            get 
            {
                return _readOnlyInitialized;
            }
        }

        public ICriteria SetReadOnly(bool readOnly)
        {
            _readOnly = readOnly;
            _readOnlyInitialized = true;
            return this;
        }
    }
}