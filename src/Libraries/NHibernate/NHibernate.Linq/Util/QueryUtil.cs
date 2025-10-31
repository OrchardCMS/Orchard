using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Linq.Expressions;
using Expression = System.Linq.Expressions.Expression;

namespace NHibernate.Linq.Util
{
	public class QueryUtil
	{
		public static object GetExpressionValue(Expression expression)
		{
			var constExpr = expression as ConstantExpression;
			if (constExpr != null)
				return constExpr.Value;

			return Expression.Lambda(typeof(Func<>).MakeGenericType(expression.Type), expression)
				.Compile().DynamicInvoke();
		}

		public static List<ParameterExpression> GetParameters(Expression expr)
		{
			var list = new List<ParameterExpression>();
			GetParameters(expr, list);
			return list;
		}

		public static object[] GetMethodParameterValues(MethodCallExpression call)
		{
			System.Type[] paramTypes = null;
			return GetMethodParameterValues(call, out paramTypes);
		}

		public static object[] GetMethodParameterValues(MethodCallExpression call, out System.Type[] types)
		{
			ConstantExpression ce;
			object[] values = null;
			types = null;

			if (call != null)
			{
				var valueList = new List<Object>();
				var typeList = new List<System.Type>();

				for (int i = 0; i < call.Arguments.Count; i++)
				{
					ce = call.Arguments[i] as ConstantExpression;
					if (ce != null)
					{
						valueList.Add(ce.Value);
						typeList.Add(ce.Type);
					}
				}
				if (valueList.Count > 0)
				{
					values = valueList.ToArray();
					types = typeList.ToArray();
				}
			}

			return values;
		}

		public static SqlFunctionExpression GetFunctionCriteria(MethodCallExpression call, ICriterion criterion,
														  SqlFunctionExpression rightFunction)
		{
			System.Type[] paramTypes = null;
			object[] paramValues = GetMethodParameterValues(call, out paramTypes);

			int propertyPosition = 0;
			string methodName = QueryUtil.GetMethodName(call, out propertyPosition);

			return new SqlFunctionExpression(methodName, call.Method.ReturnType, paramValues, paramTypes, criterion,
											 propertyPosition, rightFunction);
		}

		public static void GetParameters(Expression expr, List<ParameterExpression> list)
		{
			if (expr is ParameterExpression)
			{
				var pe = expr as ParameterExpression;
				if (!list.Contains(pe))
				{
					list.Add(pe);
				}
			}
			else if (expr is MemberExpression)
			{
				GetParameters(((MemberExpression)expr).Expression, list);
			}
			else if (expr is UnaryExpression)
			{
				GetParameters(((UnaryExpression)expr).Operand, list);
			}
			else if (expr is BinaryExpression)
			{
				var be = expr as BinaryExpression;
				GetParameters(be.Left, list);
				GetParameters(be.Right, list);
			}
		}

		public static string GetMethodName(MethodCallExpression call, out int propertyPosition)
		{
			object[] attribs = call.Method.GetCustomAttributes(typeof(SqlFunctionAttribute), false);
			var attrib = attribs.FirstOrDefault() as SqlFunctionAttribute;
			string methodName = call.Method.Name;
			propertyPosition = 0;

			if (attrib != null)
			{
				if (!String.IsNullOrEmpty(attrib.Owner))
					methodName = String.Format("{0}.{1}", attrib.Owner, methodName);
				propertyPosition = attrib.PropertyPosition;
			}
			else
			{
				// provide mapping of System methods to SQL functions
				switch (methodName.ToLower())
				{
					case "tolower":
						methodName = "lower";
						break;
					case "toupper":
						methodName = "upper";
						break;
					case "indexof":
					case "charindex":
						methodName = "charindex";
						propertyPosition = 1;
						break;
				}
			}

			return methodName;
		}
	}
}
