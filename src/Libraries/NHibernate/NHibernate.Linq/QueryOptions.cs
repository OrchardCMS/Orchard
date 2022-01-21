using System;

namespace NHibernate.Linq
{
	/// <summary>
	/// It provides methods for caching the results, and some extension methods for them.
	/// </summary>
	public class QueryOptions
	{
		private Action<ICriteria> action;

		public QueryOptions()
		{
			this.action = delegate { };
		}

		public QueryOptions SetCachable(bool cachable)
		{
			action += criteria => criteria.SetCacheable(cachable);
			return this;
		}
		public QueryOptions SetCacheMode(CacheMode mode)
		{
			action += criteria => criteria.SetCacheMode(mode);
			return this;
		}
		public QueryOptions SetCacheRegion(string cacheRegion)
		{
			action += criteria => criteria.SetCacheRegion(cacheRegion);
			return this;
		}
		public QueryOptions SetComment(string comment)
		{
			action += criteria => criteria.SetComment(comment);
			return this;
		}
		public QueryOptions RegisterCustomAction(Action<ICriteria> customAction)
		{
			action += customAction;
			return this;
		}

		internal void Execute(ICriteria criteria)
		{
			action(criteria);
		}

		public void AddExpansion(string path)
		{
			action += criteria =>
			  {
                  criteria.Fetch(SelectMode.Fetch, path);
			  };
		}
	}
}
