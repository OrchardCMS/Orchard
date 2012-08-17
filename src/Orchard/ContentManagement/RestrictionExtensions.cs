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
            ExpressionProcessor.RegisterCustomMethodCall(() => RestrictionExtensions.IsStartingWith("", ""), RestrictionExtensions.ProcessIsStartingWith);
            ExpressionProcessor.RegisterCustomMethodCall(() => RestrictionExtensions.IsEndingWith("", ""), RestrictionExtensions.ProcessIsEndingWith);
            ExpressionProcessor.RegisterCustomMethodCall(() => RestrictionExtensions.IsContaining("", ""), RestrictionExtensions.ProcessIsContaining);
        }

        public static bool IsStartingWith(this string item, string value) {
            throw new NotImplementedException("Do not use this method directly");
        }

        public static bool IsEndingWith(this string item, string value) {
            throw new NotImplementedException("Do not use this method directly");
        }

        public static bool IsContaining(this string item, string value) {
            throw new NotImplementedException("Do not use this method directly");
        }

        public static ICriterion ProcessIsStartingWith(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Arguments[0]);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[1]);
            MatchMode matchMode = MatchMode.Start;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }


        public static ICriterion ProcessIsEndingWith(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Arguments[0]);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[1]);
            MatchMode matchMode = MatchMode.End;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }


        public static ICriterion ProcessIsContaining(MethodCallExpression methodCallExpression) {
            ExpressionProcessor.ProjectionInfo projection = ExpressionProcessor.FindMemberProjection(methodCallExpression.Arguments[0]);
            string value = (string)ExpressionProcessor.FindValue(methodCallExpression.Arguments[1]);
            MatchMode matchMode = MatchMode.Anywhere;
            return projection.Create<ICriterion>(s => Restrictions.InsensitiveLike(s, value, matchMode), p => Restrictions.InsensitiveLike(p, value, matchMode));
        }
    }
}
