using System;
using System.Linq;

namespace NHibernate.Linq.Expressions
{
	public class QuerySourceExpression : NHibernateExpression
	{
		private readonly string _alias;
		private readonly IQueryable _query;
		private readonly System.Type _elementType;

		public string Alias
		{
			get { return _alias; }
		}

		public IQueryable Query
		{
			get { return _query; }
		}

		public System.Type ElementType
		{
			get { return _elementType ?? Query.ElementType; }
		}

		public QuerySourceExpression(string alias, IQueryable query)
			: this(alias, query, null) { }

		public QuerySourceExpression(string alias, IQueryable query, System.Type elementType)
			: base(NHibernateExpressionType.QuerySource, query.GetType())
		{
			_alias = alias;
			_query = query;
			_elementType = elementType;
		}

		public override string ToString()
		{
			if (!String.IsNullOrEmpty(Alias))
				return Alias;

			return base.ToString();
		}
	}
}
