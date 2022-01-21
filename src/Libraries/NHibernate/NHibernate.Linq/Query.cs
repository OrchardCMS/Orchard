using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NHibernate.Linq
{
	///<summary>
	/// Generic IQueryable base class. See http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
	/// </summary>
	public class Query<T> : INHibernateQueryable<T>
	{
		private readonly QueryProvider provider;
		private readonly Expression expression;
		private readonly QueryOptions queryOptions;

		public Query(QueryProvider provider, QueryOptions queryOptions)
		{
			if (provider == null) throw new ArgumentNullException("provider");

			this.provider = provider;
			this.queryOptions = queryOptions;
			this.expression = Expression.Constant(this);
		}

		public Query(QueryProvider provider, Expression expression, QueryOptions queryOptions)
		{
			if (provider == null) throw new ArgumentNullException("provider");
			if (expression == null) throw new ArgumentNullException("expression");

			if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
				throw new ArgumentOutOfRangeException("expression");

			this.provider = provider;
			this.queryOptions = queryOptions;
			this.expression = expression;
		}

		Expression IQueryable.Expression
		{
			get { return this.expression; }
		}

		System.Type IQueryable.ElementType
		{
			get { return typeof(T); }
		}

		IQueryProvider IQueryable.Provider
		{
			get { return this.provider; }
		}

		public QueryOptions QueryOptions
		{
			get { return queryOptions; }
		}

		public IQueryable<T> Expand(string path)
		{
			queryOptions.AddExpansion(path);

			return this;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
		}
	}
}
