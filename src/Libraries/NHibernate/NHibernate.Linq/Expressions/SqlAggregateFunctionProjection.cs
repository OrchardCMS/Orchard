using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Linq.Util;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Linq.Expressions
{
	public class SqlAggregateFunctionProjection : AggregateProjection
	{
		public SqlAggregateFunctionProjection(string functionName, string propertyName)
			: this(functionName, propertyName, null)
		{
		}

		public SqlAggregateFunctionProjection(string functionName, string propertyName, System.Type returnType)
			: this(functionName, propertyName, returnType, null)
		{
		}

		public SqlAggregateFunctionProjection(string functionName, string propertyName, System.Type returnType, object[] paramValues)
			: this(functionName, propertyName, 0, returnType, paramValues)
		{
		}

		public SqlAggregateFunctionProjection(string functionName, string propertyName, int propertyPosition, System.Type returnType,
									 object[] paramValues)
			: base(functionName, propertyName)
		{
			ReturnType = returnType;
			ParameterValues = paramValues;
			PropertyPosition = propertyPosition;
		}

		public System.Type ReturnType { get; private set; }
		public Object[] ParameterValues { get; private set; }
		public int PropertyPosition { get; private set; }

		public override IType[] GetTypes(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			if (ReturnType != null)
			{
				return new[] { TypeFactory.HeuristicType(ReturnType.Name) };
			}

			return base.GetTypes(criteria, criteriaQuery);
		}

		public override SqlString ToSqlString(ICriteria criteria, int loc, ICriteriaQuery criteriaQuery)
		{
			if (ParameterValues != null && ParameterValues.Length > 0)
			{
				var sql = new SqlStringBuilder();
				sql.Add(aggregate).Add("(");
				bool hasProperty = false;
				bool hasParameter = false;

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

					sql.Add(LinqUtil.SqlEncode(ParameterValues[i]));
					hasParameter = true;
				}
				if (!hasProperty)
				{
					if (hasParameter) sql.Add(", ");
					sql.Add(criteriaQuery.GetColumn(criteria, propertyName));
				}

				return sql.Add(") as y").Add(loc.ToString()).Add("_").ToSqlString();
			}

			// if ParameterValues were not specified, we defer to the base functionality
			return base.ToSqlString(criteria, loc, criteriaQuery);
		}
	}
}