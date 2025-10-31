using System.Linq.Expressions;
using NHibernate.Metadata;

namespace NHibernate.Linq.Expressions
{
    public class EntityExpression : NHibernateExpression
    {
        public string Alias { get; }

        public string AssociationPath { get; }

        public IClassMetadata MetaData { get; }

        public Expression Expression { get; }

        public EntityExpression(string associationPath, string alias, System.Type type, IClassMetadata metaData, Expression expression)
            : base(IsRoot(expression) ? NHibernateExpressionType.RootEntity : NHibernateExpressionType.Entity, type)
        {
            AssociationPath = associationPath;
            Alias = alias;
            MetaData = metaData;
            Expression = expression;
        }

        private static bool IsRoot(Expression expr)
        {
            if (expr == null) return true;
            if (!(expr is EntityExpression)) return true;
            return false;
        }

        public override string ToString()
        {
            return Alias;
        }

        public virtual string GetAliasedIdentifierPropertyName()
        {
            if ((NHibernateExpressionType)this.NodeType == NHibernateExpressionType.RootEntity)
            {
                return this.MetaData.IdentifierPropertyName;
            }
            return string.Format("{0}.{1}", this.Alias, this.MetaData.IdentifierPropertyName);
        }
    }
}
