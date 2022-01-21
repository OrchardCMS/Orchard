using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Linq.Expressions;
using NHibernate.Linq.Util;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Translates a Linq Expression into an NHibernate ICriteria object.
	/// </summary>
	public class NHibernateQueryTranslator : NHibernateExpressionVisitor
	{
		private readonly ISession session;
		private readonly string entityName;
		private ICriteria rootCriteria;
		private QueryOptions options;

		public NHibernateQueryTranslator(ISession session)
		{
			this.session = session;
		}
		public NHibernateQueryTranslator(ISession session,string entityName)
		{
			this.session = session;
			this.entityName = entityName;
		}

		public virtual object Translate(LinqExpression expression, QueryOptions queryOptions)
		{
			this.rootCriteria = null;
			this.options = queryOptions;

			Visit(expression); //ensure criteria

			var visitor = new RootVisitor(rootCriteria, session, true);
			visitor.Visit(expression);
			return visitor.Results;
		}

		protected override LinqExpression VisitQuerySource(QuerySourceExpression expr)
		{
			if (rootCriteria == null)
			{
				if(!string.IsNullOrEmpty(this.entityName))
					rootCriteria = session.CreateCriteria(entityName, expr.Alias);
				else
					rootCriteria = session.CreateCriteria(expr.ElementType, expr.Alias);
				options.Execute(rootCriteria);
			}
			return expr;
		}
	}
}
