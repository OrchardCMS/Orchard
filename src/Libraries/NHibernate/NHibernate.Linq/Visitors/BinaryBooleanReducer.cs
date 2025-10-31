using System.Linq.Expressions;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Preprocesses an expression tree replacing binary boolean expressions with unary expressions.
	/// </summary>
	public class BinaryBooleanReducer : ExpressionVisitor
	{
		//this class simplifies this:
		//   timesheet.Entries.Any() == true
		//to this:
		//   timesheet.Entries.Any()

		private Expression ProcessBinaryExpression(Expression exprToCompare, Expression exprToReturn, ExpressionType nodeType, Expression original)
		{
			BooleanConstantFinder visitor = new BooleanConstantFinder();
			visitor.Visit(exprToCompare);

			if (visitor.Constant.HasValue)
			{
				switch (nodeType)
				{
					case ExpressionType.Equal:
						return visitor.Constant.Value ? exprToReturn : Expression.Not(exprToReturn);
					case ExpressionType.NotEqual:
						return visitor.Constant.Value ? Expression.Not(exprToReturn) : exprToReturn;
					case ExpressionType.Or:
					case ExpressionType.OrElse:
						return visitor.Constant.Value ? Expression.Constant(true) : exprToReturn;
					case ExpressionType.And:
					case ExpressionType.AndAlso:
						return visitor.Constant.Value ? exprToReturn : Expression.Constant(false);
					default:
						return original;
				}
			}
			else
				return original;
		}

		protected override Expression VisitBinary(BinaryExpression expr)
		{
			Expression e = ProcessBinaryExpression(expr.Left, expr.Right, expr.NodeType, expr);
			if (e != expr)
				return e;
			e = ProcessBinaryExpression(expr.Right, expr.Left, expr.NodeType, expr);
			if (e != expr)
				return e;
			return base.VisitBinary(expr);

		}

		class BooleanConstantFinder : ExpressionVisitor
		{
			private bool _isNestedBinaryExpression;

			public bool? Constant { get; private set; }

			protected override Expression VisitConstant(ConstantExpression c)
			{
				if (c.Type == typeof(bool) && !_isNestedBinaryExpression)
					Constant = (bool)c.Value;
				return c;
			}

			protected override Expression VisitBinary(BinaryExpression b)
			{
				_isNestedBinaryExpression = true;
				return b;
			}
		}
	}
}
