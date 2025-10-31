using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Linq.Expressions;
using Expression = System.Linq.Expressions.Expression;
using NHProjections = NHibernate.Criterion.Projections;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Visits an expression tree providing the appropriate projections for grouping arguments.
	/// </summary>
	public class GroupingArgumentsVisitor : NHibernateExpressionVisitor
	{
		private readonly ICriteria _rootCriteria;
		private readonly List<IProjection> _projections = new List<IProjection>();

		public IProjection Projection
		{
			get
			{
				if (_projections.Count == 0)
					return null;

				if (_projections.Count == 1)
					return _projections[0];

				ProjectionList list = NHProjections.ProjectionList();
				foreach (var projection in _projections)
					list.Add(projection);

				return list;
			}
		}

		public GroupingArgumentsVisitor(ICriteria rootCriteria)
		{
			_rootCriteria = rootCriteria;
		}

		protected override Expression VisitPropertyAccess(PropertyAccessExpression expr)
		{
			string name = MemberNameVisitor.GetMemberName(_rootCriteria, expr);
			_projections.Add(NHProjections.GroupProperty(name));

			return expr;
		}

		protected override Expression VisitCollectionAccess(CollectionAccessExpression expr)
		{
			return VisitPropertyAccess(expr);
		}
	}
}
