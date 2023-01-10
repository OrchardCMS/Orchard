using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Engine;
using NHibernate.Linq.Expressions;
using NHibernate.Linq.Util;
using NHibernate.Metadata;
using NHibernate.Type;

namespace NHibernate.Linq.Visitors
{
	/// <summary>
	/// Preprocesses an expression tree replacing MemberAccessExpressions and ParameterExpressions with
	/// NHibernate-specific PropertyAccessExpressions and EntityExpressions respectively.
	/// </summary>
	public class AssociationVisitor : ExpressionVisitor
	{
		private readonly ISessionFactoryImplementor _sessionFactory;
		private readonly IDictionary<string, IClassMetadata> _metaData;
		private readonly IDictionary<System.Type, string> _proxyTypes;

		public AssociationVisitor(ISessionFactoryImplementor sessionFactory)
		{
			_sessionFactory = sessionFactory;
			_metaData = _sessionFactory.GetAllClassMetadata();
			_proxyTypes = _sessionFactory.GetProxyMetaData(_metaData);
		}

		private IClassMetadata GetMetaData(System.Type type)
		{
			if (LinqUtil.IsAnonymousType(type))
				return null;

			string entityName = _sessionFactory.TryGetGuessEntityName(type);

			if (!String.IsNullOrEmpty(entityName) && _metaData.ContainsKey(entityName))
				return _metaData[entityName];

			if (_proxyTypes.ContainsKey(type))
				return _metaData[_proxyTypes[type]];

			return null;
		}

		private EntityExpression GetParentExpression(MemberExpression expr, out string memberName, out IType nhibernateType)
		{
			memberName = null;
			nhibernateType = null;

			CollectionAccessExpression collectionExpr = expr.Expression as CollectionAccessExpression;
			if (collectionExpr != null)
			{
				return null;
			}

			PropertyAccessExpression propExpr = expr.Expression as PropertyAccessExpression;
			if (propExpr != null)
			{
				memberName = propExpr.Name + "." + expr.Member.Name;
				nhibernateType = propExpr.Expression.MetaData.GetPropertyType(memberName);
				return propExpr.Expression;
			}

			EntityExpression entityExpr = expr.Expression as EntityExpression;
			if (entityExpr != null)
			{
				memberName = expr.Member.Name;
				nhibernateType = entityExpr.MetaData.GetPropertyType(memberName);
				return entityExpr;
			}

			return null;
		}

		private string AssociationPathForEntity(MemberExpression expr)
		{
			PropertyAccessExpression propExpr = expr.Expression as PropertyAccessExpression;
			if (propExpr != null)
				return propExpr.Name + "." + expr.Member.Name;

			EntityExpression entityExpr = expr.Expression as EntityExpression;
			if (entityExpr != null && entityExpr.Expression != null)
				return entityExpr.Alias + "." + expr.Member.Name;

			return expr.Member.Name;
		}

		protected override Expression VisitMemberAccess(MemberExpression expr)
		{
			expr = (MemberExpression)base.VisitMemberAccess(expr);

			IClassMetadata metaData = GetMetaData(expr.Type);
			if (metaData != null)
			{
				string associationPath = AssociationPathForEntity(expr);
				return new EntityExpression(associationPath, expr.Member.Name, expr.Type, metaData, expr.Expression);
			}

			string memberName;
			IType nhibernateType;
			EntityExpression parentExpression = GetParentExpression(expr, out memberName, out nhibernateType);

			if (parentExpression != null)
			{
				if (nhibernateType.IsCollectionType)
				{
					CollectionType collectionType = (CollectionType)nhibernateType;
					IType nhElementType = collectionType.GetElementType((ISessionFactoryImplementor)_sessionFactory);

					System.Type elementType = nhElementType.ReturnedClass;
					IClassMetadata elementMetaData = GetMetaData(elementType);

					EntityExpression elementExpression = null;
					if (elementMetaData != null)
						elementExpression = new EntityExpression(null, memberName, elementType, elementMetaData, null);

					return new CollectionAccessExpression(memberName, expr.Type, nhibernateType, parentExpression, elementExpression);
				}

				return new PropertyAccessExpression(memberName, expr.Type, nhibernateType, parentExpression);
			}

			return expr;
		}

		protected override Expression VisitParameter(ParameterExpression expr)
		{
			IClassMetadata metaData = GetMetaData(expr.Type);
			if (metaData != null)
				return new EntityExpression(null, expr.Name, expr.Type, metaData, null);

			return expr;
		}

		protected override Expression VisitConstant(ConstantExpression expr)
		{
			IQueryable query = expr.Value as IQueryable;
			if (query != null)
			{
				return new QuerySourceExpression("this", query);
			}
			return expr;
		}
	}
}
