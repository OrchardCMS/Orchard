using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq.Util;

namespace NHibernate.Linq
{
	/// <summary>
	/// Generic IQueryProvider base class. See http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
	/// </summary>
	public abstract class QueryProvider : IQueryProvider
	{
		protected QueryOptions queryOptions;

		IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
		{
			return new Query<T>(this, expression, queryOptions);
		}

		IQueryable IQueryProvider.CreateQuery(Expression expression)
		{
			QueryOptions options = new QueryOptions();
			System.Type elementType = TypeSystem.GetElementType(expression.Type);
			return (IQueryable)Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression, options });
		}

		T IQueryProvider.Execute<T>(Expression expression)
		{
			return (T)this.Execute(expression);
		}

		object IQueryProvider.Execute(Expression expression)
		{
			return this.Execute(expression);
		}

		public abstract object Execute(Expression expression);
	}
}
