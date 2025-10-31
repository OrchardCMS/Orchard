using System.Linq;

namespace NHibernate.Linq
{
	public interface INHibernateQueryable
	{
		QueryOptions QueryOptions { get; }
	}

	public interface INHibernateQueryable<T> : INHibernateQueryable, IOrderedQueryable<T>
	{
		IQueryable<T> Expand(string path);
	}
}
