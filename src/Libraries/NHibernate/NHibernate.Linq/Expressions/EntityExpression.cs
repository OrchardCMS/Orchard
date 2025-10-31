using System.Linq.Expressions;
using NHibernate.Metadata;

namespace NHibernate.Linq.Expressions
{
	public class EntityExpression : NHibernateExpression
	{
		private readonly string _alias;
		private readonly string _associationPath;
		private readonly IClassMetadata _metaData;
		private readonly Expression _expression;

		public string Alias
		{
			get { return _alias; }
		}

		public string AssociationPath
		{
			get { return _associationPath; }
		}

		public IClassMetadata MetaData
		{
			get { return _metaData; }
		}

		public Expression Expression
		{
			get { return _expression; }
		}

		public EntityExpression(string associationPath, string alias, System.Type type, IClassMetadata metaData, Expression expression)
			: base(IsRoot(expression) ? NHibernateExpressionType.RootEntity : NHibernateExpressionType.Entity, type)
		{
			_associationPath = associationPath;
			_alias = alias;
			_metaData = metaData;
			_expression = expression;
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
