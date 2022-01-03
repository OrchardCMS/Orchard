using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Criterion;
using NHibernate.Linq.Util;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NHibernate.Linq.Visitors
{
	public interface IImmediateResultsVisitor
	{
		object GetResults(MethodCallExpression expr);
	}

	/// <summary>
	/// Visits any expression calls that require immediate results.
	/// </summary>
	public class ImmediateResultsVisitor<T> : NHibernateExpressionVisitor, IImmediateResultsVisitor
	{
		private readonly ISession session;
		private readonly ICriteria rootCriteria;
		private T results;

		public ImmediateResultsVisitor(ISession session, ICriteria rootCriteria)
		{
			this.session = session;
			this.rootCriteria = rootCriteria;
		}

		public object GetResults(MethodCallExpression expr)
		{
			Visit(expr);
			return results;
		}

		protected override LinqExpression VisitMethodCall(MethodCallExpression call)
		{
			switch (call.Method.Name)
			{
				case "First":
					results = HandleFirstCall(call);
					break;
				case "FirstOrDefault":
					results = HandleFirstOrDefaultCall(call);
					break;
				case "Single":
					results = HandleSingleCall(call);
					break;
				case "SingleOrDefault":
					results = HandleSingleOrDefaultCall(call);
					break;
				case "Aggregate":
					results = HandleAggregateCallback(call);
					break;
				case "Average":
				case "Count":
				case "LongCount":
				case "Max":
				case "Min":
				case "Sum":
					rootCriteria.ClearOrders();
					results = HandleAggregateCall(call);
					break;
				case "Any":
					results = HandleAnyCall(call);
					break;
				default:
					throw new NotImplementedException("The method " + call.Method.Name + " is not implemented.");
			}

			return call;
		}

		private T HandleFirstCall(MethodCallExpression call)
		{
			return GetElementList(call, 1).First();
		}

		private T HandleFirstOrDefaultCall(MethodCallExpression call)
		{
			return GetElementList(call, 1).FirstOrDefault();
		}

		private T HandleSingleCall(MethodCallExpression call)
		{
			return GetElementList(call, 2).Single();
		}

		private T HandleSingleOrDefaultCall(MethodCallExpression call)
		{
			return GetElementList(call, 2).SingleOrDefault();
		}

		private IList<T> GetElementList(MethodCallExpression call, int count)
		{
			if (call.Arguments.Count > 1)
				rootCriteria.Add(WhereArgumentsVisitor.GetCriterion(rootCriteria, session, call.Arguments[1]));

			return rootCriteria.SetFirstResult(0).SetMaxResults(count).List<T>();
		}

		private T HandleAggregateCallback(MethodCallExpression call)
		{
			LambdaExpression lambda = (LambdaExpression)LinqUtil.StripQuotes(call.Arguments[call.Arguments.Count - 1]);
			System.Type resultType = lambda.Parameters[1].Type;

			IList list = rootCriteria.List();
			MethodInfo castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(resultType);
			IEnumerable enumerable = (IEnumerable)castMethod.Invoke(null, new object[] { list });

			if (call.Arguments.Count == 2)
			{
				return (T)call.Method.Invoke(null, new object[]
                {
                    enumerable.AsQueryable(),
                    lambda
                });
			}
			else if (call.Arguments.Count == 3)
			{
				return (T)call.Method.Invoke(null, new[]
               	{
               		enumerable.AsQueryable(),
                    ((ConstantExpression)call.Arguments[1]).Value,
               		lambda
               	});
			}
			else if (call.Arguments.Count == 4)
			{
				return (T)call.Method.Invoke(null, new[]
               	{
               		enumerable.AsQueryable(),
                    ((ConstantExpression)call.Arguments[1]).Value,
               		LinqUtil.StripQuotes(call.Arguments[2]),
                    lambda
               	});
			}

			throw new ArgumentException("Invalid number of arguments passed to the Aggregate method.");
		}

		private T HandleAggregateCall(MethodCallExpression call)
		{
			var visitor = new SelectArgumentsVisitor(rootCriteria, session);
			visitor.Visit(call);

			T value = default(T);
			if (visitor.Projection != null)
			{
				object result = rootCriteria.SetProjection(visitor.Projection).UniqueResult();
				if (result != null)
				{
					value = (T)LinqUtil.ChangeType(result, typeof(T));
				}
			}

			return value;
		}

		private T HandleAnyCall(MethodCallExpression call)
		{
			rootCriteria.SetProjection(Projections.RowCount());

			if (call.Arguments.Count > 1)
				rootCriteria.Add(WhereArgumentsVisitor.GetCriterion(rootCriteria, session, call.Arguments[1]));

			int count = (int)rootCriteria.UniqueResult();

			//HACK: the Any method always returns bool - maybe need to make this class non-generic
			return (T)(object)(count > 0);
		}
	}
}
