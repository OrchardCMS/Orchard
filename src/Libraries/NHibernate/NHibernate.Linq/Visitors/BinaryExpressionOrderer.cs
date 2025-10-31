using System.Linq.Expressions;
using NHibernate.Linq.Expressions;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Preprocesses an expression tree ordering binary expressions in accordance with the <see cref="T:NHibernate.Linq.Visitors.BinaryCriterionVisitor"/>.
	/// </summary>
	public class BinaryExpressionOrderer : NHibernateExpressionVisitor
	{
		// This class makes sure binary expressions are in one of these configurations
		// so that the BinaryCriterionVisitor can correctly process it.
		// LEFT           RIGHT
		// ========================
		// property       value
		// property       property
		// property       criteria
		// value          criteria
		// criteria       criteria

		private ExpressionType ReflectExpressionType(ExpressionType type)
		{
			switch (type)
			{
				case ExpressionType.LessThan:
					return ExpressionType.GreaterThan;

				case ExpressionType.LessThanOrEqual:
					return ExpressionType.GreaterThanOrEqual;

				case ExpressionType.GreaterThan:
					return ExpressionType.LessThan;

				case ExpressionType.GreaterThanOrEqual:
					return ExpressionType.LessThanOrEqual;

				case ExpressionType.Equal:
                    return ExpressionType.Equal;

				case ExpressionType.NotEqual:
                    return ExpressionType.NotEqual;

				default:
					return type;
			}
		}

		private Expression Swap(BinaryExpression expr)
		{
			ExpressionType nodeType = ReflectExpressionType(expr.NodeType);
			return Expression.MakeBinary(nodeType, expr.Right, expr.Left, expr.IsLiftedToNull, expr.Method, expr.Conversion);
		}

		protected override Expression VisitBinary(BinaryExpression expr)
		{
			BinaryExpressionTypeFinder left = new BinaryExpressionTypeFinder();
			left.Visit(expr.Left);

			if (left.Type == BinaryCriterionType.None)
				return base.VisitBinary(expr);

			BinaryExpressionTypeFinder right = new BinaryExpressionTypeFinder();
			right.Visit(expr.Right);

			if (right.Type == BinaryCriterionType.None)
				return base.VisitBinary(expr);


			if (right.Type == BinaryCriterionType.Property)
			{
				if (left.Type == BinaryCriterionType.Criteria
					|| left.Type == BinaryCriterionType.Value)
				{
					return Swap(expr);
				}
			}
			else if (right.Type == BinaryCriterionType.Value)
			{
				if (left.Type == BinaryCriterionType.Criteria)
				{
					return Swap(expr);
				}
			}

			return expr;
		}

		class BinaryExpressionTypeFinder : NHibernateExpressionVisitor
		{
			private bool _isNestedBinaryExpression;
			private BinaryCriterionType _type;

			public BinaryCriterionType Type
			{
				get { return _type; }
				private set
				{
					if (!_isNestedBinaryExpression)
						_type = value;
				}
			}

			protected override Expression VisitBinary(BinaryExpression b)
			{
				_isNestedBinaryExpression = true;
				return b;
			}

			protected override Expression VisitMethodCall(MethodCallExpression expr)
			{
				Type = BinaryCriterionType.Criteria;
				return expr;
			}

			protected override Expression VisitConstant(ConstantExpression expr)
			{
				Type = BinaryCriterionType.Value;
				return expr;
			}

			protected override Expression VisitEntity(EntityExpression expr)
			{
				Type = BinaryCriterionType.Property;
				return expr;
			}

			protected override Expression VisitPropertyAccess(PropertyAccessExpression expr)
			{
				Type = BinaryCriterionType.Property;
				return expr;
			}

			protected override Expression VisitCollectionAccess(CollectionAccessExpression expr)
			{
				Type = BinaryCriterionType.Property;
				return expr;
			}
		}
	}
}
