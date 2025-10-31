using System;
using NHibernate.Type;

namespace NHibernate.Linq.Expressions
{
	public class PropertyAccessExpression : NHibernateExpression
	{
		private readonly string _name;
		private readonly EntityExpression _expression;
		private readonly IType _nhibernateType;

		public string Name
		{
			get { return _name; }
		}

		public EntityExpression Expression
		{
			get { return _expression; }
		}

		public IType NHibernateType
		{
			get { return _nhibernateType; }
		}

		public PropertyAccessExpression(string name, System.Type type, IType nhibernateType, EntityExpression expression)
			: this(name, type, nhibernateType, expression, NHibernateExpressionType.PropertyAccess) { }

		protected PropertyAccessExpression(string name, System.Type type, IType nhibernateType, EntityExpression expression, NHibernateExpressionType nodeType)
			: base(nodeType, type)
		{
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");
			if (nhibernateType == null) throw new ArgumentNullException("nhibernateType");
			if (expression == null) throw new ArgumentNullException("expression");

			_name = name;
			_expression = expression;
			_nhibernateType = nhibernateType;
		}

		public override string ToString()
		{
			return this.Expression.ToString() + "." + this.Name;
		}
	}
}
