using System.Linq.Expressions;
using NHibernate.Linq.Expressions;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Retrieves the first (or root) instance of EntityExpression found in the given Expression.
	/// </summary>
	public class EntityExpressionVisitor : NHibernateExpressionVisitor
	{
		private readonly bool _findFirstEntity;

		public EntityExpression Expression { get; private set; }

		public EntityExpressionVisitor(bool findFirstEntity)
		{
			_findFirstEntity = findFirstEntity;
		}

		protected override Expression VisitEntity(EntityExpression expr)
		{
			this.Expression = expr;
			if (_findFirstEntity) return expr;
			return base.VisitEntity(expr);
		}

		protected override Expression VisitMethodCall(MethodCallExpression expr)
		{
			Visit(expr.Arguments[0]);
			return expr;
		}

		private static EntityExpression FindEntity(Expression expr, bool findFirst)
		{
			EntityExpressionVisitor visitor = new EntityExpressionVisitor(findFirst);
			visitor.Visit(expr);
			return visitor.Expression;
		}

		public static EntityExpression FirstEntity(Expression expr)
		{
			return FindEntity(expr, true);
		}

		public static EntityExpression RootEntity(Expression expr)
		{
			return FindEntity(expr, false);
		}
	}
}
