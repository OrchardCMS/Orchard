using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NHibernate.Linq.Util
{
	/// <summary>
	/// Provides static utility methods that aid in evaluating expression trees.
	/// </summary>
	public static class LinqUtil
	{
		/// <summary>
		/// Creates a collection of type T by invoking a delegate method during
		/// enumeration that return each item, begining with an initialValue.
		/// </summary>
		/// <typeparam name="T">The type of collection being created.</typeparam>
		/// <param name="func">A delegate method to invoke.</param>
		/// <param name="initialValue">The first item in the collection.</param>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1"/> collection of type T.</returns>
		public static IEnumerable<T> Iterate<T>(Func<T, T> func, T initialValue)
		{
			T value = initialValue;
			while (true)
			{
				yield return value;
				value = func(value);
			}
		}

		/// <summary>
		/// Returns an <see cref="T:System.Object"/> with the specified <see cref="T:System.Type"/>
		/// and whose value is equivalent to the specified object.
		/// </summary>
		/// <param name="value">An <see cref="T:System.Object"/> that implements the <see cref="T:System.IConvertible"/> interface.</param>
		/// <param name="conversionType">A <see cref="T:System.Type"/>.</param>
		/// <returns>An object whose <see cref="T:System.Type"/> is conversionType and whose value is equivalent
		/// to value, or null, if value is null and conversionType is not a value type.</returns>
		public static object ChangeType(object value, System.Type conversionType)
		{
			// have to use IsAssignableFrom() due to proxy classes
			if (value != null && !conversionType.IsAssignableFrom(value.GetType()))
			{
				if (IsNullableType(conversionType))
				{
					System.Type arg = conversionType.GetGenericArguments()[0];
					if (arg.IsEnum)
					{
						if (value is string)
						{
							value = Activator.CreateInstance(conversionType, Enum.Parse(arg, value as string));
						}
						else
						{
							value = Activator.CreateInstance(conversionType, Enum.ToObject(arg, value));
						}
					}
					else
					{
						value = Activator.CreateInstance(conversionType, Convert.ChangeType(value, arg));
					}
				}
				else
				{
					if (conversionType.IsEnum)
					{
						if (value is string)
						{
							value = Enum.Parse(conversionType, value as string);
						}
						else
						{
							value = Enum.ToObject(conversionType, value);
						}
					}
					else
					{
						value = Convert.ChangeType(value, conversionType);
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Determines if the specified type is a <see cref="T:System.Nullable`1"/> type.
		/// </summary>
		/// <param name="type">A <see cref="T:System.Type"/> to check.</param>
		/// <returns>True if the type is a <see cref="T:System.Nullable`1"/> type, otherwise false.</returns>
		public static bool IsNullableType(System.Type type)
		{
			return type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		/// <summary>
		/// Determines if the specified type is an anonymous type.
		/// </summary>
		/// <param name="type">A <see cref="T:System.Type"/> to check.</param>
		/// <returns>True if the type is an anonymous type, otherwise false.</returns>
		public static bool IsAnonymousType(System.Type type)
		{
			return type != null && type.Name.StartsWith("<");
		}

		/// <summary>
		/// Encodes an <see cref="T:System.Object"/> for use in SQL statements.
		/// </summary>
		/// <param name="value">The value to encode.</param>
		/// <returns>A SQL encoded value.</returns>
		public static string SqlEncode(object value)
		{
			if (value != null)
			{
				System.Type type = value.GetType();

				if (IsNullableType(type))
				{
					value = type.GetProperty("Value").GetValue(value, null);
					return SqlEncode(value);
				}

				switch (System.Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
						return ((bool)value) ? "1" : "0";
					case TypeCode.Byte:
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.Single:
						return value.ToString();
					case TypeCode.DBNull:
					case TypeCode.Empty:
						return "null";
					default:
						return String.Format("'{0}'", value.ToString().Replace("'", "''"));
				}
			}

			return "null";
		}

		public static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
				e = ((UnaryExpression)e).Operand;
			return e;
		}
	}
}
