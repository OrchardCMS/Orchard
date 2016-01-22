using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Orchard.DisplayManagement.Shapes {
    public class Composite : DynamicObject {

        private readonly IDictionary _props = new HybridDictionary();

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return TryGetMemberImpl(binder.Name, out result);
        }

        protected virtual bool TryGetMemberImpl(string name, out object result) {
            if (_props.Contains(name)) {
                result = _props[name];
                return true;
            }

            result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            return TrySetMemberImpl(binder.Name, value);
        }

        protected bool TrySetMemberImpl(string name, object value) {
            _props[name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            if (!args.Any()) {
                return TryGetMemberImpl(binder.Name, out result);
            }

            // method call with one argument will assign the property
            if (args.Count() == 1) {
                result = this;
                return TrySetMemberImpl(binder.Name, args.Single());
            }

            if (!base.TryInvokeMember(binder, args, out result)) {
                if (binder.Name == "ToString") {
                    result = string.Empty;
                    return true;
                }

                return false;
            }

            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            if (indexes.Count() != 1) {
                return base.TryGetIndex(binder, indexes, out result);
            }

            var index = indexes.Single();

            if (_props.Contains(index)) {
                result = _props[index];
                return true;
            }

            // try to access an existing member
            var strinIndex = index as string;

            if (strinIndex != null && TryGetMemberImpl(strinIndex, out result)) {
                return true;
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            if (indexes.Count() != 1) {
                return base.TrySetIndex(binder, indexes, value);
            }

            var index = indexes.Single();

            // try to access an existing member
            var strinIndex = index as string;

            if (strinIndex != null && TrySetMemberImpl(strinIndex, value)) {
                return true;
            }

            _props[indexes.Single()] = value;
            return true;
        }

        public IDictionary Properties {
            get { return _props; }
        }

        public static bool operator ==(Composite a, Nil b) {
            return null == a;
        }

        public static bool operator !=(Composite a, Nil b) {
            return !(a == b);
        }

        protected bool Equals(Composite other) {
            return Equals(_props, other._props);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return Equals((Composite)obj);
        }

        public override int GetHashCode() {
            return (_props != null ? _props.GetHashCode() : 0);
        }

        #region InterfaceProxyBehavior
        private static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();
        static readonly MethodInfo DynamicMetaObjectProviderGetMetaObject = typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject");

        public override bool TryConvert(ConvertBinder binder, out object result) {
            var type = binder.ReturnType;

            if (type.IsInterface && type != typeof(IDynamicMetaObjectProvider)) {
                var proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithoutTarget(
                    type,
                    new[] { typeof(IDynamicMetaObjectProvider) },
                    ProxyGenerationOptions.Default);

                var interceptors = new IInterceptor[] { new Interceptor(this) };
                var proxy = Activator.CreateInstance(proxyType, new object[] { interceptors, this });
                result = proxy;
                return true;
            }

            result = null;
            return false;
        }

        private class Interceptor : IInterceptor {
            public object Self { get; private set; }

            public Interceptor(object self) {
                Self = self;
            }

            public void Intercept(IInvocation invocation) {
                if (invocation.Method == DynamicMetaObjectProviderGetMetaObject) {
                    var expression = (Expression)invocation.Arguments.Single();
                    invocation.ReturnValue = new ForwardingMetaObject(
                        expression,
                        BindingRestrictions.Empty,
                        invocation.Proxy,
                        (IDynamicMetaObjectProvider)Self,
                        exprProxy => Expression.Field(exprProxy, "__target"));

                    return;
                }

                var invoker = BindInvoker(invocation);
                invoker(invocation);

            }


            private static readonly ConcurrentDictionary<MethodInfo, Action<IInvocation>> Invokers = new ConcurrentDictionary<MethodInfo, Action<IInvocation>>();

            private static Action<IInvocation> BindInvoker(IInvocation invocation) {
                return Invokers.GetOrAdd(invocation.Method, CompileInvoker);
            }

            private static Action<IInvocation> CompileInvoker(MethodInfo method) {

                var methodParameters = method.GetParameters();
                var invocationParameter = Expression.Parameter(typeof(IInvocation), "invocation");

                var targetAndArgumentInfos = Pack(
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    methodParameters.Select(
                        mp => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name))).ToArray();

                var targetAndArguments = Pack<Expression>(
                    Expression.Coalesce(
                        Expression.Property(invocationParameter, invocationParameter.Type, "InvocationTarget"), 
                        Expression.Property(invocationParameter, invocationParameter.Type, "Proxy")),
                    methodParameters.Select(
                        (mp, index) =>
                        Expression.Convert(
                            Expression.ArrayIndex(
                                Expression.Property(invocationParameter, invocationParameter.Type,
                                                    "Arguments"),
                                Expression.Constant(index)), mp.ParameterType))).ToArray();

                Expression body = null;
                if (method.IsSpecialName) {
                    if (method.Name.Equals("get_Item")) {
                        body = Expression.Dynamic(
                            Binder.GetIndex(
                                CSharpBinderFlags.InvokeSpecialName,
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.Equals("set_Item")) {

                        var targetAndArgumentInfosWithoutTheNameValue = Pack(
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                            methodParameters.Select(
                                mp => mp.Name == "value" ? CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) : CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, mp.Name)));

                        body = Expression.Dynamic(
                            Binder.SetIndex(
                                CSharpBinderFlags.InvokeSpecialName,
                                typeof(object),
                                targetAndArgumentInfosWithoutTheNameValue),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.StartsWith("get_")) {
                        //  Build lambda containing the following call site:
                        //  (IInvocation invocation) => {
                        //      invocation.ReturnValue = (object) ((dynamic)invocation.InvocationTarget).{method.Name};
                        //  }
                        body = Expression.Dynamic(
                            Binder.GetMember(
                                CSharpBinderFlags.InvokeSpecialName,
                                method.Name.Substring("get_".Length),
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }

                    if (body == null && method.Name.StartsWith("set_")) {
                        body = Expression.Dynamic(
                            Binder.SetMember(
                                CSharpBinderFlags.InvokeSpecialName,
                                method.Name.Substring("set_".Length),
                                typeof(object),
                                targetAndArgumentInfos),
                            typeof(object),
                            targetAndArguments);
                    }
                }
                if (body == null) {
                    //  Build lambda containing the following call site:
                    //  (IInvocation invocation) => {
                    //      invocation.ReturnValue = (object) ((dynamic)invocation.InvocationTarget).{method.Name}(
                    //          {methodParameters[*].Name}: ({methodParameters[*].Type})invocation.Arguments[*],
                    //          ...);
                    //  }


                    body = Expression.Dynamic(
                        Binder.InvokeMember(
                            CSharpBinderFlags.None,
                            method.Name,
                            null,
                            typeof(object),
                            targetAndArgumentInfos),
                        typeof(object),
                        targetAndArguments);
                }

                if (method.ReturnType != typeof(void)) {
                    body = Expression.Assign(
                        Expression.Property(invocationParameter, invocationParameter.Type, "ReturnValue"),
                        Expression.Convert(body, typeof(object)));
                }

                var lambda = Expression.Lambda<Action<IInvocation>>(body, invocationParameter);
                return lambda.Compile();
            }

        }

        static IEnumerable<T> Pack<T>(T t1) {
            if (!Equals(t1, default(T)))
                yield return t1;
        }
        static IEnumerable<T> Pack<T>(T t1, IEnumerable<T> t2) {
            if (!Equals(t1, default(T)))
                yield return t1;
            foreach (var t in t2)
                yield return t;
        }
        static IEnumerable<T> Pack<T>(T t1, IEnumerable<T> t2, T t3) {
            if (!Equals(t1, default(T)))
                yield return t1;
            foreach (var t in t2)
                yield return t;
            if (!Equals(t3, default(T)))
                yield return t3;
        }

        /// <summary>
        /// Based on techniques discussed by Tomáš Matoušek
        /// at http://blog.tomasm.net/2009/11/07/forwarding-meta-object/
        /// </summary>
        public sealed class ForwardingMetaObject : DynamicMetaObject {
            private readonly DynamicMetaObject _metaForwardee;

            public ForwardingMetaObject(Expression expression, BindingRestrictions restrictions, object forwarder,
                IDynamicMetaObjectProvider forwardee, Func<Expression, Expression> forwardeeGetter)
                : base(expression, restrictions, forwarder) {

                // We'll use forwardee's meta-object to bind dynamic operations.
                _metaForwardee = forwardee.GetMetaObject(
                    forwardeeGetter(
                        Expression.Convert(expression, forwarder.GetType())   // [1]
                    )
                );
            }

            // Restricts the target object's type to TForwarder. 
            // The meta-object we are forwarding to assumes that it gets an instance of TForwarder (see [1]).
            // We need to ensure that the assumption holds.
            private DynamicMetaObject AddRestrictions(DynamicMetaObject result) {
                var restricted = new DynamicMetaObject(
                    result.Expression,
                    BindingRestrictions.GetTypeRestriction(Expression, Value.GetType()).Merge(result.Restrictions),
                    _metaForwardee.Value
                    );
                return restricted;
            }

            // Forward all dynamic operations or some of them as needed //

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                return AddRestrictions(_metaForwardee.BindGetMember(binder));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value) {
                return AddRestrictions(_metaForwardee.BindSetMember(binder, value));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder) {
                return AddRestrictions(_metaForwardee.BindDeleteMember(binder));
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes) {
                return AddRestrictions(_metaForwardee.BindGetIndex(binder, indexes));
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value) {
                return AddRestrictions(_metaForwardee.BindSetIndex(binder, indexes, value));
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes) {
                return AddRestrictions(_metaForwardee.BindDeleteIndex(binder, indexes));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindInvokeMember(binder, args));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindInvoke(binder, args));
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args) {
                return AddRestrictions(_metaForwardee.BindCreateInstance(binder, args));
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder) {
                return AddRestrictions(_metaForwardee.BindUnaryOperation(binder));
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg) {
                return AddRestrictions(_metaForwardee.BindBinaryOperation(binder, arg));
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder) {
                return AddRestrictions(_metaForwardee.BindConvert(binder));
            }


        }
        #endregion 


    }

    public class Nil : DynamicObject {
        static readonly Nil Singleton = new Nil();
        public static Nil Instance { get { return Singleton; } }

        private Nil() {
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = Instance;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            result = Instance;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            result = Nil.Instance;
            return true;
        }


        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result) {
            switch (binder.Operation) {
                case ExpressionType.Equal:
                    result = ReferenceEquals(arg, Nil.Instance) || (object)arg == null;
                    return true;
                case ExpressionType.NotEqual:
                    result = !ReferenceEquals(arg, Nil.Instance) && (object)arg != null;
                    return true;
            }

            return base.TryBinaryOperation(binder, arg, out result);
        }

        public static bool operator ==(Nil a, Nil b) {
            return true;
        }

        public static bool operator !=(Nil a, Nil b) {
            return false;
        }

        public static bool operator ==(Nil a, object b) {
            return ReferenceEquals(a, b) || (object)b == null;
        }

        public static bool operator !=(Nil a, object b) {
            return !(a == b);
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return true;
            }

            return ReferenceEquals(obj, Nil.Instance);
        }

        public override int GetHashCode() {
            return 0;
        }

        public override bool TryConvert(ConvertBinder binder, out object result) {
            result = null;
            return true;
        }

        public override string ToString() {
            return string.Empty;
        }        
    }
}
