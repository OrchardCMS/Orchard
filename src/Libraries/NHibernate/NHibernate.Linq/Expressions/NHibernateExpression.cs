using System.Linq.Expressions;

namespace NHibernate.Linq.Expressions
{
	public abstract class NHibernateExpression : Expression
	{
		new public NHibernateExpressionType NodeType
		{
			get { return (NHibernateExpressionType)base.NodeType; }
		}

		public NHibernateExpression(NHibernateExpressionType nodeType, System.Type type)
			: base((ExpressionType)nodeType, type) { }
	}
}
