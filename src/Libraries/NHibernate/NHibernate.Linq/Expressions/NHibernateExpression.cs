using System;
using System.Linq.Expressions;

namespace NHibernate.Linq.Expressions
{
	public abstract class NHibernateExpression : Expression
	{

		public NHibernateExpression(NHibernateExpressionType nodeType, System.Type type)
			: base() {

            _nodeType = nodeType;
            _type = type;
        }

        private readonly NHibernateExpressionType _nodeType;
        public override ExpressionType NodeType { get { return (ExpressionType)_nodeType; } }
        private readonly System.Type _type;
        public override System.Type Type { get { return _type; } }
    }
}
