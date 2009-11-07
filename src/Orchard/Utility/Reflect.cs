using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Validation;

namespace Orchard.Utility {
    /// <summary>
    /// Allows static reflection on members (methods, fields, properties).
    /// This code has been adapted from the following blog post:
    /// http://wekempf.spaces.live.com/blog/cns!D18C3EC06EA971CF!694.entry
    /// </summary>
    public static class Reflect {
        public static MemberInfo GetMember(Expression<Action> expression) {
            Argument.ThrowIfNull(expression, "expression");

            return GetMemberInfo(expression);
        }

        public static MemberInfo GetMember<T>(Expression<Func<T>> expression) {
            Argument.ThrowIfNull(expression, "expression");

            return GetMemberInfo(expression);
        }

        public static MethodInfo GetMethod(Expression<Action> expression) {
            MethodInfo method = GetMember(expression) as MethodInfo;
            Argument.ThrowIfNull(method, "expression", "Expression is not a method call");

            return method;
        }

        public static PropertyInfo GetProperty<T>(Expression<Func<T>> expression) {
            PropertyInfo property = GetMember(expression) as PropertyInfo;
            Argument.ThrowIfNull(property, "expression", "Expression is not a property");

            return property;
        }

        public static FieldInfo GetField<T>(Expression<Func<T>> expression) {
            FieldInfo field = GetMember(expression) as FieldInfo;
            Argument.ThrowIfNull(field, "expression", "Expression is not a field access");

            return field;
        }

        public static string NameOf<T>(T value, Expression<Action<T>> expression) {
            return GetNameOf(expression.Body);
        }

        public static string NameOf<T, TResult>(T value, Expression<Func<T, TResult>> expression) {
            return GetNameOf(expression.Body);
        }

        internal static MemberInfo GetMemberInfo(LambdaExpression lambda) {
            Argument.ThrowIfNull(lambda, "lambda");

            if (lambda.Body.NodeType == ExpressionType.Call) {
                return ((MethodCallExpression)lambda.Body).Method;
            }

            MemberExpression memberExpression = GetMemberExpression(lambda.Body);
            Argument.ThrowIfNull(memberExpression, "lambda", "Expression is not a member access");

            return memberExpression.Member;
        }

        internal static MemberExpression GetMemberExpression(Expression expression) {
            MemberExpression memberExpression = null;
            if (expression.NodeType == ExpressionType.Convert) {
                memberExpression = ((UnaryExpression)expression).Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess) {
                memberExpression = expression as MemberExpression;
            }
            return memberExpression;
        }

        internal static string GetNameOf(Expression expression) {
            MemberExpression memberExpression = GetMemberExpression(expression);
            if (memberExpression == null) {
                LambdaExpression lambda = expression as LambdaExpression;
                if (lambda == null)
                    return null;
                return GetMemberInfo(lambda).Name;
            }
            string parentName = GetNameOf(memberExpression.Expression);
            if (parentName == null)
                return memberExpression.Member.Name;
            return parentName + "." + memberExpression.Member.Name;
        }
    }
}