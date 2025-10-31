using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace NHibernate.Linq
{
	/// <summary>
	/// Provides a static method that enables LINQ syntax for NHibernate Criteria Queries.
	/// </summary>
	public static class NHibernateExtensions
	{
		/// <summary>
		/// Creates a new <see cref="T:NHibernate.Linq.NHibernateQueryProvider"/> object used to evaluate an expression tree.
		/// </summary>
		/// <typeparam name="T">An NHibernate entity type.</typeparam>
		/// <param name="session">An initialized <see cref="T:NHibernate.ISession"/> object.</param>
		/// <returns>An <see cref="T:NHibernate.Linq.NHibernateQueryProvider"/> used to evaluate an expression tree.</returns>
		public static INHibernateQueryable<T> Linq<T>(this ISession session)
		{
			QueryOptions options = new QueryOptions();
			return new Query<T>(new NHibernateQueryProvider(session, options), options);
		}

		public static INHibernateQueryable<T> Linq<T>(this ISession session,string entityName)
		{
			QueryOptions options = new QueryOptions();
			return new Query<T>(new NHibernateQueryProvider(session, options,entityName), options);
		}

		public static void List<T>(this ISession session, Expression expr, IList list)
		{
			var options = new QueryOptions();
			var queryProvider = new NHibernateQueryProvider(session, options);
			IQueryable<T> queryable = new Query<T>(queryProvider, options);
			queryable = queryable.Where((Expression<Func<T, bool>>)expr);

			var result = queryProvider.TranslateExpression(queryable.Expression);
			var criteria = result as ICriteria;
			if (criteria != null)
			{
				criteria.List(list);
			}
			else
			{
				var items = result as IEnumerable;
				if (items != null)
				{
					foreach (var item in items)
					{
						list.Add(item);
					}
				}
			}
		}
	}
}