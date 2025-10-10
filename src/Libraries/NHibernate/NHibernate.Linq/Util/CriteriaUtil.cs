using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Loader.Criteria;
using NHibernate.Persister.Entity;
using NHibernate.Transform;

namespace NHibernate.Linq.Util
{
	public static class CriteriaUtil
	{
		#region Extension Methods

		public static ICriteriaQuery GenerateCriteriaQuery(this ICriteria criteria, ISessionFactory sessionFactory, string rootEntityName)
		{
			return new CriteriaQueryTranslator(
				(ISessionFactoryImplementor)sessionFactory,
				(CriteriaImpl)criteria,
				rootEntityName, "this");
		}

		public static void SetProjectionIfNotNull(this ICriteria criteria, IProjection projection)
		{
			if (projection != null)
				criteria.SetProjection(projection);
		}

		public static void SetResultTransformerIfNotNull(this ICriteria criteria, IResultTransformer transformer)
		{
			if (transformer != null)
				criteria.SetResultTransformer(transformer);
			else
				criteria.SetResultTransformer(new RootEntityResultTransformer());
		}

		public static void Add(this ICriteria criteria, IEnumerable<ICriterion> criterion)
		{
			foreach (ICriterion c in criterion)
			{
				criteria.Add(c);
			}
		}

		public static string GetEntityOrClassName(this ICriteria criteria)
		{
			if (criteria is CriteriaImpl)
			{
				return ((CriteriaImpl)criteria).EntityOrClassName;
			}

			if (criteria is DetachedCriteriaAdapter)
			{
				var adapter = (DetachedCriteriaAdapter)criteria;
				return adapter.DetachedCriteria.EntityOrClassName;
			}
			throw new NotSupportedException("criteria must be of type CriteriaImpl or DetachedCriteriaAdapter.");
		}

		public static IProjection GetProjection(this ICriteria criteria)
		{
			var impl = criteria as CriteriaImpl;
			if (impl != null)
			{
				return impl.Projection;
			}
			return null;
		}

		public static System.Type GetRootType(this ICriteria criteria)
		{
			if (criteria is DetachedCriteriaAdapter)
			{
				var adapter = (DetachedCriteriaAdapter)criteria;
				return GetRootType(adapter.DetachedCriteria, adapter.Session);
			}
			return GetRootType(GetRootCriteria(criteria));
		}

		#endregion

		public static ISessionImplementor GetSession(ICriteria criteria)
		{
			return GetRootCriteria(criteria).Session;
		}

		private static CriteriaImpl GetRootCriteria(ICriteria criteria)
		{
			var impl = criteria as CriteriaImpl;
			if (impl != null)
				return impl;
			return GetRootCriteria(((CriteriaImpl.Subcriteria)criteria).Parent);
		}

		private static System.Type GetRootType(CriteriaImpl criteria)
		{
			if (criteria.Session == null)
				throw new InvalidOperationException("Could not get root type on criteria that is not attached to a session");

			ISessionFactoryImplementor factory = criteria.Session.Factory;

			//TODO: need to cache the entityName meta data
			var entityNames = factory.GetEntityNameMetaData();

			if (!entityNames.ContainsKey(criteria.EntityOrClassName))
				throw new InvalidOperationException("Could not find entity named: " + criteria.EntityOrClassName);

			return entityNames[criteria.EntityOrClassName];
		}

		private static System.Type GetRootType(DetachedCriteria criteria, ISession session)
		{
			ISessionFactoryImplementor factory = (ISessionFactoryImplementor)session.SessionFactory;
			IEntityPersister persister = factory.GetEntityPersister(criteria.EntityOrClassName);
			if (persister == null)
				throw new InvalidOperationException("Could not find entity named: " + criteria.EntityOrClassName);

			return persister.MappedClass;
		}
	}
}
