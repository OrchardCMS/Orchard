using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Impl;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {
    public static class RestrictionExtensions {
        public static void RegisterExtensions() {
            ExpressionProcessor.RegisterCustomMethodCall(() => "".StartsWith(""), RestrictionExtensions.ProcessStartsWith);
            ExpressionProcessor.RegisterCustomMethodCall(() => "".EndsWith(""), RestrictionExtensions.ProcessEndsWith);
            ExpressionProcessor.RegisterCustomMethodCall(() => "".Contains(""), RestrictionExtensions.ProcessContains);
        }

        public static ICriterion ProcessStartsWith(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Object);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[0]);
            MatchMode matchMode = MatchMode.Start;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }


        public static ICriterion ProcessEndsWith(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Object);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[0]);
            MatchMode matchMode = MatchMode.End;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }


        public static ICriterion ProcessContains(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Object);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[0]);
            MatchMode matchMode = MatchMode.Anywhere;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }
    }
}
