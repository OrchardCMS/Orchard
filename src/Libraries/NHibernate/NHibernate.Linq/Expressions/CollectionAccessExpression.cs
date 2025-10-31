using NHibernate.Type;

namespace NHibernate.Linq.Expressions
{
    public class CollectionAccessExpression : PropertyAccessExpression
    {
        public EntityExpression ElementExpression { get; }

        public CollectionAccessExpression(string name, System.Type type, IType nhibernateType,
            EntityExpression expression, EntityExpression elementExpression)
            : base(name, type, nhibernateType, expression, NHibernateExpressionType.CollectionAccess)
        {
            ElementExpression = elementExpression;
        }
    }
}
