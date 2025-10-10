using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Linq.Expressions;
using NHibernate.Linq.Util;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Assigns the appropriate aliases to a collection access.
	/// </summary>
	public class CollectionAliasVisitor : NHibernateExpressionVisitor
	{
		private IEnumerable<ParameterExpression> _currentCollectionAliases;

		private string FindCollectionAlias(System.Type elementType)
		{
			if (_currentCollectionAliases == null) return null;

			foreach (ParameterExpression p in _currentCollectionAliases)
			{
				if (p.Type == elementType)
				{
					return p.Name;
				}
			}

			return null;
		}

		protected override Expression VisitCollectionAccess(CollectionAccessExpression expr)
		{
			EntityExpression elementExpression = expr.ElementExpression;
			if (elementExpression == null)
				return expr;

			string alias = FindCollectionAlias(elementExpression.Type);

			if (String.IsNullOrEmpty(alias))
				return expr;

			elementExpression = new EntityExpression(elementExpression.AssociationPath, alias, elementExpression.Type, elementExpression.MetaData, elementExpression.Expression);
			return new CollectionAccessExpression(expr.Name, expr.Type, expr.NHibernateType, expr.Expression, elementExpression);
		}

		protected override Expression VisitMethodCall(MethodCallExpression expr)
		{
			if (expr.Arguments.Count > 1)
			{
				LambdaExpression lambda = LinqUtil.StripQuotes(expr.Arguments[1]) as LambdaExpression;
				if (lambda != null)
				{
					_currentCollectionAliases = lambda.Parameters;
				}
			}

			return base.VisitMethodCall(expr);
		}

		public static Expression AssignCollectionAccessAliases(Expression expr)
		{
			CollectionAliasVisitor visitor = new CollectionAliasVisitor();
			return visitor.Visit(expr);
		}
	}
}
