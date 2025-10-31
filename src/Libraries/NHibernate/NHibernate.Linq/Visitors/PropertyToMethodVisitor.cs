using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Linq.Expressions;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Converts calls to an IEnumerable.Count property to IEnumerable.Count() extension method.
	/// </summary>
	public class PropertyToMethodVisitor : NHibernateExpressionVisitor
	{
		private MethodInfo GetCountMethod(System.Type elementType)
		{
			//this should work, but it doesn't...
			//System.Type genericEnumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
			//return typeof(Enumerable).GetMethod("Count", new System.Type[] { genericEnumerableType });

			var method = typeof(Enumerable).GetMethods()
				.Where(m => m.Name == "Count" && m.GetParameters().Count() == 1).First();
			return method.MakeGenericMethod(elementType);
		}

		private Expression CastIfNecessary(CollectionAccessExpression expr)
		{
			if (expr.Type.IsGenericType)
				return expr;

			MethodInfo method = typeof(Enumerable).GetMethod("Cast")
				.MakeGenericMethod(expr.ElementExpression.Type);

			return Expression.Call(method, expr);
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			CollectionAccessExpression parent = m.Expression as CollectionAccessExpression;
			if (parent != null && m.Member.Name == "Count")
			{
				MethodInfo method = GetCountMethod(parent.ElementExpression.Type);
				return Expression.Call(method, CastIfNecessary(parent));
			}
			return base.VisitMemberAccess(m);
		}
	}
}
