using System;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Linq.Expressions;
using NHibernate.Linq.Util;
using Expression = System.Linq.Expressions.Expression;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Visits a BinaryExpression providing the appropriate NHibernate ICriterion.
	/// </summary>
	public class BinaryCriterionVisitor : NHibernateExpressionVisitor
	{
		private readonly ICriteria rootCriteria;
		private readonly ISession session;

		public BinaryCriterionVisitor(ICriteria rootCriteria, ISession session)
		{
			this.rootCriteria = rootCriteria;
			this.session = session;
		}

		public System.Type ConvertTo { get; private set; }

		public BinaryCriterionType Type { get; private set; }

		public object Value { get; private set; }

		public string Name { get; private set; }

		public DetachedCriteria Criteria { get; private set; }

		protected override Expression VisitMethodCall(MethodCallExpression expr)
		{
			Type = BinaryCriterionType.Criteria;

			//TODO: don't hardcode this alias 'sub'
			Criteria = DetachedCriteria.ForEntityName(rootCriteria.GetEntityOrClassName(), "sub");

			EntityExpression rootEntity = EntityExpressionVisitor.RootEntity(expr);
			if (rootEntity != null)
			{
				string identifierName = rootEntity.MetaData.IdentifierPropertyName;
				Criteria.Add(Restrictions.EqProperty(rootCriteria.Alias + "." + identifierName, "sub." + identifierName));
			}

			if (SelectArgumentsVisitor.SupportsMethod(expr.Method.Name))
			{
				var projectionVisitor = new SelectArgumentsVisitor(Criteria.Adapt(session), session);
				projectionVisitor.Visit(expr);
				Criteria.SetProjection(projectionVisitor.Projection);
			}

			return expr;
		}

		protected override Expression VisitMemberAccess(MemberExpression expr)
		{
			Type = BinaryCriterionType.Property;
			Name = expr.Member.Name;
			return expr;
		}

		protected override Expression VisitConstant(ConstantExpression expr)
		{
			Type = BinaryCriterionType.Value;
			Value = QueryUtil.GetExpressionValue(expr);
			return expr;
		}

		protected override Expression VisitEntity(EntityExpression expr)
		{
			Type = BinaryCriterionType.Property;
			Name = MemberNameVisitor.GetMemberName(rootCriteria, expr);
			return expr;
		}

		protected override Expression VisitPropertyAccess(PropertyAccessExpression expr)
		{
			Type = BinaryCriterionType.Property;
			Name = MemberNameVisitor.GetMemberName(rootCriteria, expr);
			return expr;
		}

		protected override Expression VisitCollectionAccess(CollectionAccessExpression expr)
		{
			return VisitPropertyAccess(expr);
		}

		protected override Expression VisitUnary(UnaryExpression expr)
		{
			if (expr.NodeType == ExpressionType.Convert)
			{
				//convert to the type of the operand, not the type of the conversion
				ConvertTo = expr.Operand.Type;
				Visit(expr.Operand);
			}

			return expr;
		}

		public static ICriterion GetBinaryCriteria(
			ICriteria rootCriteria,
			ISession session,
			BinaryExpression expr,
			ComparePropToValue comparePropToValue,
			ComparePropToProp comparePropToProp,
			CompareValueToCriteria compareValueToCriteria,
			ComparePropToCriteria comparePropToCriteria)
		{
			var left = new BinaryCriterionVisitor(rootCriteria, session);
			var right = new BinaryCriterionVisitor(rootCriteria, session);

			left.Visit(expr.Left);
			right.Visit(expr.Right);

			//the query should have been preprocessed so that
			//only the following combinations are possible:
			// LEFT           RIGHT
			// ========================
			// property       value
			// property       property
			// property       criteria
			// value          criteria
			// criteria       criteria   <== not supported yet

			switch (left.Type)
			{
				case BinaryCriterionType.Property:
					switch (right.Type)
					{
						case BinaryCriterionType.Value:
							object val = right.Value;
							if (left.ConvertTo != null)
								val = LinqUtil.ChangeType(val, left.ConvertTo);
							return comparePropToValue(left.Name, val);

						case BinaryCriterionType.Property:
							return comparePropToProp(left.Name, right.Name);

						case BinaryCriterionType.Criteria:
							return comparePropToCriteria(left.Name, right.Criteria);
					}
					break;

				case BinaryCriterionType.Value:
					return compareValueToCriteria(left.Value, right.Criteria);
			}

			throw new NotSupportedException("Could not understand: " + expr);
		}
	}
}