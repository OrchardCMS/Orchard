using System;
using NHibernate.Type;

namespace NHibernate.Linq.Expressions
{
    public class PropertyAccessExpression : NHibernateExpression
    {
        public string Name { get; }

        public EntityExpression Expression { get; }

        public IType NHibernateType { get; }

        public PropertyAccessExpression(string name, System.Type type, IType nhibernateType, EntityExpression expression)
            : this(name, type, nhibernateType, expression, NHibernateExpressionType.PropertyAccess) { }

        protected PropertyAccessExpression(string name, System.Type type, IType nhibernateType, EntityExpression expression, NHibernateExpressionType nodeType)
            : base(nodeType, type)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            if (nhibernateType == null) throw new ArgumentNullException("nhibernateType");
            if (expression == null) throw new ArgumentNullException("expression");

            Name = name;
            Expression = expression;
            NHibernateType = nhibernateType;
        }

        public override string ToString()
        {
            return this.Expression.ToString() + "." + this.Name;
        }
    }
}
