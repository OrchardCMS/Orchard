using System;
using System.Collections.Generic;
using System.Reflection;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Linq.Expressions
{
	public class SqlFunctionExpression : ICriterion
	{
		private String op;

		public SqlFunctionExpression(String functionName, System.Type returnType, ICriterion innerCriterion)
			: this(functionName, returnType, null, null, innerCriterion, 0, null)
		{
		}

		public SqlFunctionExpression(String functionName, System.Type returnType, Object[] paramValues,
									 System.Type[] paramTypes, ICriterion innerCriterion)
			: this(functionName, returnType, paramValues, paramTypes, innerCriterion, 0, null)
		{
		}

		public SqlFunctionExpression(String functionName, System.Type returnType, Object[] paramValues,
									 System.Type[] paramTypes, ICriterion innerCriterion, int propertyPosition)
			: this(functionName, returnType, paramValues, paramTypes, innerCriterion, propertyPosition, null)
		{
		}

		public SqlFunctionExpression(String functionName, System.Type returnType, Object[] paramValues,
									 System.Type[] paramTypes, ICriterion innerCriterion, int propertyPosition,
									 SqlFunctionExpression rightFunction)
		{
			FunctionName = functionName;
			ReturnType = returnType;
			ParameterValues = paramValues;
			ParameterTypes = paramTypes;
			InnerCriterion = innerCriterion;
			PropertyPosition = propertyPosition;
			RightFunction = rightFunction;
		}

		private SqlFunctionExpression RightFunction { get; set; }
		public ICriterion InnerCriterion { get; set; }
		public String FunctionName { get; private set; }
		public System.Type ReturnType { get; private set; }
		public Object[] ParameterValues { get; private set; }
		public System.Type[] ParameterTypes { get; private set; }
		public int PropertyPosition { get; private set; }

		protected virtual string Op
		{
			get
			{
				if (String.IsNullOrEmpty(op))
				{
					op = InnerCriterion.GetType().GetProperty("Op", BindingFlags.NonPublic | BindingFlags.Instance)
							.GetValue(InnerCriterion, null) as String;
				}

				return op;
			}
		}

		#region ICriterion Members

		public virtual TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			var values = new List<TypedValue>();
			if (ParameterValues != null)
			{
				for (int i = 0; i < ParameterValues.Length; i++)
				{
					values.Add(new TypedValue(TypeFactory.HeuristicType(ParameterTypes[i].Name), ParameterValues[i]));
				}
			}
			if (ReturnType != null && InnerCriterion is SimpleExpression)
			{
				var simple = InnerCriterion as SimpleExpression;
				values.Add(new TypedValue(TypeFactory.HeuristicType(ReturnType.Name), simple.Value));
			}
			if (RightFunction != null)
			{
				values.AddRange(RightFunction.GetTypedValues(criteria, criteriaQuery));
			}

			return values.ToArray();
		}

		public virtual SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			var sql = new SqlStringBuilder();
			string leftPropertyName = null;
			string rightPropertyName = null;

			if (InnerCriterion is SimpleExpression)
			{
				leftPropertyName = ((SimpleExpression)InnerCriterion).PropertyName;
			}
			else if (InnerCriterion is PropertyExpression)
			{
				System.Type type = typeof(PropertyExpression);
				leftPropertyName =
					type.GetField("_lhsPropertyName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(InnerCriterion) as
					String;
				rightPropertyName =
					type.GetField("_rhsPropertyName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(InnerCriterion) as
					String;
			}

			AddParameters(leftPropertyName, sql, criteria, criteriaQuery);
			sql.Add(" ").Add(Op).Add(" ");

			if (RightFunction != null)
			{
				RightFunction.AddParameters(rightPropertyName, sql, criteria, criteriaQuery);
			}
			else
			{
				sql.AddParameter();
			}

			return sql.ToSqlString();
		}

		public IProjection[] GetProjections()
		{
			return null;
		}

		#endregion

		private void AddParameters(String propertyName, SqlStringBuilder sql, ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			bool hasProperty = false;
			bool hasParameter = false;

			sql.Add(FunctionName).Add("(");

			if (ParameterValues != null && ParameterValues.Length > 0)
			{
				for (int i = 0; i < ParameterValues.Length; i++)
				{
					if (PropertyPosition == i)
					{
						if (i > 0) sql.Add(", ");
						sql.Add(criteriaQuery.GetColumn(criteria, propertyName)).Add(", ");
						hasProperty = true;
					}
					else if (i > 0)
					{
						sql.Add(", ");
					}

					sql.AddParameter();
					hasParameter = true;
				}
			}
			if (!hasProperty)
			{
				if (hasParameter) sql.Add(", ");
				sql.Add(criteriaQuery.GetColumn(criteria, propertyName));
			}

			sql.Add(")");
		}
	}
}