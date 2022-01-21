using System;
using System.Linq.Expressions;
using NHibernate.Engine;
using NHibernate.Linq.Util;
using NHibernate.Linq.Visitors;

namespace NHibernate.Linq
{
	public class NHibernateQueryProvider : QueryProvider
	{
		private readonly ISession _session;
		private readonly string entityName;

		public NHibernateQueryProvider(ISession session, QueryOptions queryOptions)
		{
			if (session == null) throw new ArgumentNullException("session");
			_session = session;
			this.queryOptions = queryOptions;
		}

		public NHibernateQueryProvider(ISession session, QueryOptions queryOptions,string entityName)
		{
			if (session == null) throw new ArgumentNullException("session");
			_session = session;
			this.entityName = entityName;
			this.queryOptions = queryOptions;
		}


		private static object ResultsFromCriteria(ICriteria criteria, Expression expression)
		{
			System.Type elementType = TypeSystem.GetElementType(expression.Type);

			return Activator.CreateInstance(typeof(CriteriaResultReader<>)
			  .MakeGenericType(elementType), criteria);
		}

		public object TranslateExpression(Expression expression)
		{
			expression = Evaluator.PartialEval(expression);
			expression = new BinaryBooleanReducer().Visit(expression);
			expression = new AssociationVisitor((ISessionFactoryImplementor)_session.SessionFactory).Visit(expression);
			expression = new InheritanceVisitor().Visit(expression);
			expression = CollectionAliasVisitor.AssignCollectionAccessAliases(expression);
			expression = new PropertyToMethodVisitor().Visit(expression);
			expression = new BinaryExpressionOrderer().Visit(expression);

			NHibernateQueryTranslator translator = new NHibernateQueryTranslator(_session,entityName);
			return translator.Translate(expression, this.queryOptions);
		}

		public override object Execute(Expression expression)
		{
			var results = TranslateExpression(expression);
			var criteria = results as ICriteria;

			if (criteria != null)
				return ResultsFromCriteria(criteria, expression);
			return results;
		}
	}
}