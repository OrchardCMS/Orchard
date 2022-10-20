using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq.Expressions;
using NHibernate.Type;

namespace NHibernate.Linq.Visitors {
    /// <summary>
    /// When DateTime date is identified by NHibernate to be in its UtcDateTimeType
    /// format, they cannot be compared to DateTime whose Kind property is Unspecified.
    /// As a consequence, queries that used to work in previous versions of NHibernate
    /// would work no more.
    /// </summary>
    public class UtcDateTimeExpressionVisitor : NHibernateExpressionVisitor {

        protected override Expression VisitBinary(BinaryExpression expr) {
            // check whether either (but not both) the arguments of the expression are
            // of type UtcDateTimeType
            var leftIsUtc = expr.Left is PropertyAccessExpression
                && (((PropertyAccessExpression)expr.Left).NHibernateType is UtcDateTimeType);
            var rightIsUtc = expr.Right is PropertyAccessExpression
                && (((PropertyAccessExpression)expr.Right).NHibernateType is UtcDateTimeType);
            if ((leftIsUtc && rightIsUtc) || (!leftIsUtc && !rightIsUtc)) {
                // both true or both false
                return base.VisitBinary(expr);
            }
            // Only at most one side is UtcDateTimeType
            // => the other side should be parsed to make sure it's compatible
            Expression left, right;
            Expression conversion = Visit(expr.Conversion);
            if (leftIsUtc) {
                left = Visit(expr.Left);
                // "transform" right branch
                right = new UtcConstantsTransformer().Visit(expr.Right);
            } else { //if (rightIsUtc) 
                // "transform" left branch
                left = new UtcConstantsTransformer().Visit(expr.Left);
                right = Visit(expr.Right);
            }
            // Actual BinaryExpression logic
            if (left != expr.Left || right != expr.Right || conversion != expr.Conversion) {
                if (expr.NodeType == ExpressionType.Coalesce && expr.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(expr.NodeType, left, right, expr.IsLiftedToNull, expr.Method);
            }
            return expr;
        }

        class UtcConstantsTransformer : UtcDateTimeExpressionVisitor {

            protected override Expression VisitConstant(ConstantExpression c) {
                if (c.Value is DateTime) {
                    return Expression.Constant(
                        new DateTime(((DateTime)(c.Value)).Ticks, DateTimeKind.Utc),
                        c.Type);
                }
                return base.VisitConstant(c);
            }
        }
    }
}
