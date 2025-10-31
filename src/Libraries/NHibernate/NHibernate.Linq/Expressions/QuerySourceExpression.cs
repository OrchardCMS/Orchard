using System.Linq;

namespace NHibernate.Linq.Expressions
{
    public class QuerySourceExpression : NHibernateExpression
    {
        private readonly string _alias;
        private readonly IQueryable _query;
        private readonly System.Type _elementType;

        public string Alias => _alias;

        public IQueryable Query => _query;

        public System.Type ElementType => _elementType ?? Query.ElementType;

        public QuerySourceExpression(string alias, IQueryable query)
            : this(alias, query, null) { }

        public QuerySourceExpression(string alias, IQueryable query, System.Type elementType)
            : base(NHibernateExpressionType.QuerySource, query.GetType())
        {
            _alias = alias;
            _query = query;
            _elementType = elementType;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Alias))
                return Alias;

            return base.ToString();
        }
    }
}
