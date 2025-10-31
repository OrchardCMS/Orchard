using System;

namespace NHibernate.Linq.Expressions
{
	/// <summary>
	/// Associates a method with a corresponding SQL function.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class SqlFunctionAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:NHibernate.Linq.Expressions.SqlFunctionAttribute"/> class.
		/// </summary>
		public SqlFunctionAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:NHibernate.Linq.Expressions.SqlFunctionAttribute"/> class.
		/// </summary>
		/// <param name="owner">The name of the schema that owns the SQL function.</param>
		public SqlFunctionAttribute(string owner)
		{
			Owner = owner;
		}

		/// <summary>
		/// Gets or sets the name of the schema that owns the SQL function.
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// Gets or sets the position of the function parameter that accepts the property name.
		/// </summary>
		public int PropertyPosition { get; set; }
	}
}