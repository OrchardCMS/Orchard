using System;
using System.Linq.Expressions;
using System.Reflection;
using Orchard.Validation;

namespace Orchard.Utility {
    /// <summary>
    /// Allows static reflection on members (methods, fields, properties).
    /// This code has been adapted from the following blog post:
    /// http://wekempf.spaces.live.com/blog/cns!D18C3EC06EA971CF!694.entry
    /// Example:
    /// class Program {
    ///    public void Foo() { }
    ///    static void Main(string[] args) {
    ///        Console.WriteLine(ReflectOn&lt;Progra&gt;.GetMethod(p =&gt; p.Foo()).Name);
    ///    }
    /// }
    /// </summary>
    public static class ReflectOn<T> {
        public static MemberInfo GetMember(Expression<Action<T>> expression) {
            Argument.ThrowIfNull(expression, "expression");

            return Reflect.GetMemberInfo(expression);
        }

        public static MemberInfo GetMember<TResult>(Expression<Func<T, TResult>> expression) {
            Argument.ThrowIfNull(expression, "expression");

            return Reflect.GetMemberInfo(expression);
        }

        public static MethodInfo GetMethod(Expression<Action<T>> expression) {
            MethodInfo method = GetMember(expression) as MethodInfo;
            Argument.ThrowIfNull(method, "expression", "Expression is not a method call");

            return method;
        }

        public static PropertyInfo GetProperty<TResult>(Expression<Func<T, TResult>> expression) {
            PropertyInfo property = GetMember(expression) as PropertyInfo;
            Argument.ThrowIfNull(property, "expression", "Expression is not a property access");

            return property;
        }

        public static FieldInfo GetField<TResult>(Expression<Func<T, TResult>> expression) {
            FieldInfo field = GetMember(expression) as FieldInfo;
            Argument.ThrowIfNull(field, "expression", "Expression is not a field access");

            return field;
        }

        public static string NameOf(Expression<Action<T>> expression) {
            return Reflect.GetNameOf(expression);
        }

        public static string NameOf<TResult>(Expression<Func<T, TResult>> expression) {
            return Reflect.GetNameOf(expression);
        }
    }
}