using System.Diagnostics;
using System.Linq.Expressions;
using NHibernate.Linq.Expressions;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// NHibernate-specific base expression visitor.
	/// </summary>
	[DebuggerStepThrough, DebuggerNonUserCode]
	public class NHibernateExpressionVisitor : ExpressionVisitor
	{
		public override Expression Visit(Expression exp)
		{
			if (exp == null) return null;

			switch ((NHibernateExpressionType)exp.NodeType)
			{
				case NHibernateExpressionType.QuerySource:
					return VisitQuerySource((QuerySourceExpression)exp);
				case NHibernateExpressionType.RootEntity:
				case NHibernateExpressionType.Entity:
					return VisitEntity((EntityExpression)exp);
				case NHibernateExpressionType.PropertyAccess:
					return VisitPropertyAccess((PropertyAccessExpression)exp);
				case NHibernateExpressionType.CollectionAccess:
					return VisitCollectionAccess((CollectionAccessExpression)exp);
				default:
					return base.Visit(exp);
			}
		}

		protected virtual Expression VisitQuerySource(QuerySourceExpression expr)
		{
			return expr;
		}

		protected virtual Expression VisitEntity(EntityExpression expr)
		{
			Expression e = Visit(expr.Expression);
			if (e != expr.Expression)
			{
				return new EntityExpression(expr.AssociationPath, expr.Alias, expr.Type, expr.MetaData, e);
			}
			return expr;
		}

		protected virtual Expression VisitPropertyAccess(PropertyAccessExpression expr)
		{
			EntityExpression e = (EntityExpression)Visit(expr.Expression);
			if (e != expr.Expression)
			{
				return new PropertyAccessExpression(expr.Name, expr.Type, expr.NHibernateType, e);
			}
			return expr;
		}

		protected virtual Expression VisitCollectionAccess(CollectionAccessExpression expr)
		{
			EntityExpression e = (EntityExpression)Visit(expr.Expression);
			if (e != expr.Expression)
			{
				return new CollectionAccessExpression(expr.Name, expr.Type, expr.NHibernateType, e, expr.ElementExpression);
			}
			return expr;
		}
	}
}
