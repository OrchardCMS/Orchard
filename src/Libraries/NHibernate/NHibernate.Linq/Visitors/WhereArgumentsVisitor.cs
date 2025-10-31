using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Linq.Expressions;
using NHibernate.Linq.Util;
using NHibernate.Persister.Entity;
using Expression = System.Linq.Expressions.Expression;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Provides ICriterion for a query given a Linq expression tree.
	/// </summary>
	public class WhereArgumentsVisitor : NHibernateExpressionVisitor
	{
		private readonly Stack<IList<ICriterion>> criterionStack = new Stack<IList<ICriterion>>();
		private readonly ICriteria rootCriteria;
		private readonly ISession session;

		public WhereArgumentsVisitor(ICriteria rootCriteria, ISession session)
		{
			criterionStack.Push(new List<ICriterion>());
			this.rootCriteria = rootCriteria;
			this.session = session;
		}

		public static IEnumerable<ICriterion> GetCriterion(ICriteria rootCriteria, ISession session, Expression expression)
		{
			var visitor = new WhereArgumentsVisitor(rootCriteria, session);
			visitor.Visit(expression);
			return visitor.CurrentCriterions;
		}

		/// <summary>
		/// Gets the current collection of <see cref="T:NHibernate.Criterion.ICriterion"/> objects.
		/// </summary>
		public IList<ICriterion> CurrentCriterions
		{
			get { return criterionStack.Peek(); }
		}

		public static bool SupportsMethod(string methodName)
		{
			return "Any".Equals(methodName)
				|| "StartsWith".Equals(methodName)
				|| "EndsWith".Equals(methodName)
				|| "Contains".Equals(methodName)
				|| "Equals".Equals(methodName);
		}

		protected override Expression VisitConstant(ConstantExpression expr)
		{
			if (expr.Type == typeof(bool))
			{
				bool value = (bool)expr.Value;
				var falseCriteria = NHibernate.Criterion.Expression.Sql("1=0");
				var trueCriteria = NHibernate.Criterion.Expression.Sql("1=1");
				if (value)
				{
					this.CurrentCriterions.Add(trueCriteria);
				}
				else
				{
					this.CurrentCriterions.Add(falseCriteria);
				}

				return expr;
			}

			if (expr.Type.IsSubclassOf(typeof(Expression)))
				return Visit(expr.Value as Expression);

			return base.VisitConstant(expr);
		}

		protected override Expression VisitMethodCall(MethodCallExpression expr)
		{
			switch (expr.Method.Name)
			{
				case "Any":
					CurrentCriterions.Add(GetExistsCriteria(expr));
					break;
				case "StartsWith":
					CurrentCriterions.Add(GetLikeCriteria(expr, MatchMode.Start));
					break;
				case "EndsWith":
					CurrentCriterions.Add(GetLikeCriteria(expr, MatchMode.End));
					break;
				case "Contains":
					if (expr.Object == null)
					{
						if (expr.Arguments[0] is ConstantExpression)
						{
							CurrentCriterions.Add(GetCollectionContainsCriteria(expr.Arguments[0], expr.Arguments[1]));
						}
						else if (expr.Arguments[0] is CollectionAccessExpression)
						{
							CurrentCriterions.Add(GetCollectionContainsCriteria((CollectionAccessExpression)expr.Arguments[0], expr.Arguments[1]));
						}
						else
						{
							//myArray.Contains(user.Name)
							CurrentCriterions.Add(GetCollectionContainsCriteria(expr.Arguments[0], expr.Arguments[1]));
						}
					}
					else if (expr.Object is ConstantExpression)
					{
						//myList.Contains(user.Name)
						CurrentCriterions.Add(GetCollectionContainsCriteria(expr.Object, expr.Arguments[0]));
					}
					else if (expr.Object is CollectionAccessExpression)
					{
						//timesheet.Entries.Contains(entry)
						CurrentCriterions.Add(GetCollectionContainsCriteria((CollectionAccessExpression)expr.Object, expr.Arguments[0]));
					}
					else
					{
						//user.Name.Contains(partialName)
						CurrentCriterions.Add(GetLikeCriteria(expr, MatchMode.Anywhere));
					}
					break;
				case "Equals":
					VisitBinaryCriterionExpression(Expression.Equal(expr.Object, expr.Arguments[0]));
					break;
			}

			return expr;
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expr)
		{
			var visitor = new MemberNameVisitor(rootCriteria);
			visitor.Visit(expr);
			string memberName = visitor.MemberName + ".class";

			var metaData = session.SessionFactory.GetClassMetadata(expr.TypeOperand);
			if (metaData.HasSubclasses)
			{
				//make sure to include any subtypes
				var disjunction = new Disjunction();
				foreach (string entityName in ((IEntityPersister)metaData).EntityMetamodel.SubclassEntityNames)
				{
					var metadata = session.SessionFactory.GetClassMetadata(entityName);
					disjunction.Add(Property.ForName(memberName).Eq(metadata.MappedClass));
				}
				visitor.CurrentCriteria.Add(disjunction);
			}
			else
			{
				visitor.CurrentCriteria.Add(Property.ForName(memberName).Eq(expr.TypeOperand));
			}

			return expr;
		}

		protected override Expression VisitBinary(BinaryExpression expr)
		{
			switch (expr.NodeType)
			{
				case ExpressionType.AndAlso:
					VisitAndAlsoExpression(expr);
					break;

				case ExpressionType.OrElse:
					VisitOrElseExpression(expr);
					break;

				default:
					VisitBinaryCriterionExpression(expr);
					break;
			}

			return expr;
		}

		private void VisitAndAlsoExpression(BinaryExpression expr)
		{
			criterionStack.Push(new List<ICriterion>());
			Visit(expr.Left);
			Visit(expr.Right);
			var ands = criterionStack.Pop();

			var conjunction = new Conjunction();
			foreach (var crit in ands)
				conjunction.Add(crit);
			CurrentCriterions.Add(conjunction);
		}

		private void VisitOrElseExpression(BinaryExpression expr)
		{
			criterionStack.Push(new List<ICriterion>());
			Visit(expr.Left);
			Visit(expr.Right);
			IList<ICriterion> ors = criterionStack.Pop();

			var disjunction = new Disjunction();
			foreach (ICriterion crit in ors)
			{
				disjunction.Add(crit);
			}
			CurrentCriterions.Add(disjunction);
		}

		private void VisitBinaryCriterionExpression(BinaryExpression expr)
		{
			ComparePropToValue comparePropToValue = null;
			ComparePropToProp comparePropToProp = null;
			CompareValueToCriteria compareValueToCriteria = null;
			ComparePropToCriteria comparePropToCriteria = null;

			switch (expr.NodeType)
			{
				case ExpressionType.Equal:
					comparePropToValue = (n, v) => (v != null) ? Restrictions.Eq(n, v) : Restrictions.IsNull(n);
					comparePropToProp = Restrictions.EqProperty;
					compareValueToCriteria = Subqueries.Eq;
					comparePropToCriteria = Subqueries.PropertyEq;
					break;

				case ExpressionType.GreaterThan:
					comparePropToValue = Restrictions.Gt;
					comparePropToProp = Restrictions.GtProperty;
					compareValueToCriteria = Subqueries.Gt;
					comparePropToCriteria = Subqueries.PropertyGt;
					break;

				case ExpressionType.GreaterThanOrEqual:
					comparePropToValue = Restrictions.Ge;
					comparePropToProp = Restrictions.GeProperty;
					compareValueToCriteria = Subqueries.Ge;
					comparePropToCriteria = Subqueries.PropertyGe;
					break;

				case ExpressionType.LessThan:
					comparePropToValue = Restrictions.Lt;
					comparePropToProp = Restrictions.LtProperty;
					compareValueToCriteria = Subqueries.Lt;
					comparePropToCriteria = Subqueries.PropertyLt;
					break;

				case ExpressionType.LessThanOrEqual:
					comparePropToValue = Restrictions.Le;
					comparePropToProp = Restrictions.LeProperty;
					compareValueToCriteria = Subqueries.Le;
					comparePropToCriteria = Subqueries.PropertyLe;
					break;

				case ExpressionType.NotEqual:
					comparePropToValue = (n, v) => (v != null) ? Restrictions.Not(Restrictions.Eq(n, v)) : Restrictions.IsNotNull(n);
					comparePropToProp = Restrictions.NotEqProperty;
					compareValueToCriteria = Subqueries.Ne;
					comparePropToCriteria = Subqueries.PropertyNe;
					break;
			}

			CurrentCriterions.Add(
				BinaryCriterionVisitor.GetBinaryCriteria(rootCriteria, session,
					expr, comparePropToValue, comparePropToProp,
					compareValueToCriteria, comparePropToCriteria));
		}

		protected override Expression VisitUnary(UnaryExpression expr)
		{
			switch (expr.NodeType)
			{
				case ExpressionType.Quote:
					Visit(expr.Operand);
					break;

				case ExpressionType.Not:
					VisitNotExpression(expr);
					break;
			}

			return expr;
		}

		private void VisitNotExpression(UnaryExpression expr)
		{
			var criterions = GetCriterion(rootCriteria, session, expr.Operand);

			Conjunction conjunction = Restrictions.Conjunction();
			foreach (var criterion in criterions)
				conjunction.Add(criterion);

			CurrentCriterions.Add(Restrictions.Not(conjunction));
		}

		protected override Expression VisitPropertyAccess(PropertyAccessExpression expr)
		{
			if (expr.Type == typeof(bool))
			{
				string name = MemberNameVisitor.GetMemberName(rootCriteria, expr);
				CurrentCriterions.Add(Restrictions.Eq(name, true));
			}

			return expr;
		}

		private ICriterion GetExistsCriteria(MethodCallExpression expr)
		{
			EntityExpression rootEntity = EntityExpressionVisitor.FirstEntity(expr);
			string propertyName = MemberNameVisitor.GetMemberName(rootCriteria, expr);

			DetachedCriteria query = DetachedCriteria.For(rootEntity.Type)
				.SetProjection(Projections.Id())
				.Add(Restrictions.IsNotEmpty(propertyName));

			if (expr.Arguments.Count > 1)
			{
				var arg = (LambdaExpression)LinqUtil.StripQuotes(expr.Arguments[1]);
				string alias = arg.Parameters[0].Name;

				DetachedCriteria subquery = query.CreateCriteria(propertyName, alias);

				var temp = new WhereArgumentsVisitor(subquery.Adapt(session), session);
				temp.Visit(arg.Body);

				foreach (ICriterion c in temp.CurrentCriterions)
				{
					subquery.Add(c);
				}
			}

			string identifierName = rootEntity.GetAliasedIdentifierPropertyName();
			return Subqueries.PropertyIn(identifierName, query);
		}

		private ICriterion GetLikeCriteria(MethodCallExpression expr, MatchMode matchMode)
		{
			return Restrictions.Like(MemberNameVisitor.GetMemberName(rootCriteria, expr.Object),
									 String.Format("{0}", QueryUtil.GetExpressionValue(expr.Arguments[0])),
									 matchMode);
		}

		private ICriterion GetCollectionContainsCriteria(CollectionAccessExpression arg, Expression containsExpression)
		{
			EntityExpression rootEntity = EntityExpressionVisitor.FirstEntity(arg);

			DetachedCriteria query = DetachedCriteria.For(rootEntity.Type)
				.SetProjection(Projections.Id());

			var visitor = new MemberNameVisitor(query.Adapt(session), true);
			visitor.Visit(arg);

			//TODO: this won't work for collections of values
			var containedEntity = QueryUtil.GetExpressionValue(containsExpression);
			var collectionIdPropertyName = visitor.MemberName + "." + arg.ElementExpression.MetaData.IdentifierPropertyName;
			var idValue = arg.ElementExpression.MetaData.GetIdentifier(containedEntity);

			query.Add(Restrictions.Eq(collectionIdPropertyName, idValue));

			string identifierName = rootEntity.MetaData.IdentifierPropertyName;
			return Subqueries.PropertyIn(identifierName, query);
		}

		private ICriterion GetCollectionContainsCriteria(Expression list, Expression containedExpr)
		{
			var values = QueryUtil.GetExpressionValue(list) as ICollection;

			if (values == null)
				throw new InvalidOperationException("Expression argument must be of type ICollection.");

			return Restrictions.In(MemberNameVisitor.GetMemberName(rootCriteria, containedExpr),
									   values);
		}
	}
}