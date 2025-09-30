using NHibernate.Criterion;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Represents a method that returns an <see cref="T:NHibernate.Criterion.ICriterion"/>
	/// object that compares one property to another property using a binary expression.
	/// </summary>
	/// <param name="propertyName">The name of the property to compare on the left hand side of the expression.</param>
	/// <param name="otherPropertyName">The name of the property to compare on the right hand side of the expression.</param>
	/// <returns>An initialized <see cref="T:NHibernate.Criterion.ICriterion"/> object.</returns>
	public delegate ICriterion ComparePropToProp(string propertyName, string otherPropertyName);

	/// <summary>
	/// Represents a method that returns an <see cref="T:NHibernate.Criterion.ICriterion"/>
	/// object that compares a property to a constant value using a binary expression.
	/// </summary>
	/// <param name="propertyName">The name of the property to compare on the left hand side of the expression.</param>
	/// <param name="value">The constant value used for the right hand side of the expression.</param>
	/// <returns>An initialized <see cref="T:NHibernate.Criterion.ICriterion"/> object.</returns>
	public delegate ICriterion ComparePropToValue(string propertyName, object value);

	/// <summary>
	/// Represents a method that returns an <see cref="T:NHibernate.Criterion.ICriterion"/>
	/// object that compares a value to a criteria using a binary expression.
	/// </summary>
	/// <param name="value">The value on the left hand side of the expression.</param>
	/// <param name="criteria">The <see cref="T:NHibernate.Criterion.DetachedCriteria"/> used for the right hand side of the expression.</param>
	/// <returns>An initialized <see cref="T:NHibernate.Criterion.ICriterion"/> object.</returns>
	public delegate ICriterion CompareValueToCriteria(object value, DetachedCriteria criteria);

	/// <summary>
	/// Represents a method that returns an <see cref="T:NHibernate.Criterion.ICriterion"/>
	/// object that compares a property to a criteria using a binary expression.
	/// </summary>
	/// <param name="propertyName">The name of the property to compare on the left hand side of the expression.</param>
	/// <param name="criteria">The <see cref="T:NHibernate.Criterion.DetachedCriteria"/> used for the right hand side of the expression.</param>
	/// <returns>An initialized <see cref="T:NHibernate.Criterion.ICriterion"/> object.</returns>
	public delegate ICriterion ComparePropToCriteria(string propertyName, DetachedCriteria criteria);
}
