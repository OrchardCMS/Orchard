using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Implementation;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Orchard.DisplayManagement {
    public class DefaultShapeTableFactory : IShapeTableFactory {
        private readonly IEnumerable<IShapeDriver> _shapeProviders;

        public DefaultShapeTableFactory(IEnumerable<IShapeDriver> shapeProviders) {
            _shapeProviders = shapeProviders;
        }

        public ShapeTable CreateShapeTable() {
            var table = new ShapeTable { Entries = GetEntries().ToDictionary(e => e.ShapeType) };
            return table;
        }

        private IEnumerable<ShapeTable.Entry> GetEntries() {
            foreach (var shapeProvider in _shapeProviders) {
                foreach (var methodInfo in shapeProvider.GetType().GetMethods().Where(IsAcceptableMethod)) {
                    var info = methodInfo;
                    var provider = shapeProvider;
                    yield return new ShapeTable.Entry {
                        ShapeType = methodInfo.Name,
                        Target = ctx => PerformInvoke(ctx, info, provider)
                    };
                }
            }
        }

        private object PerformInvoke(DisplayContext displayContext, MethodInfo methodInfo, IShapeDriver shapeDriver) {
            // oversimplification for the sake of evolving
            dynamic shape = displayContext.Value;
            var arguments = methodInfo.GetParameters()
                .Select(parameter => BindParameter(displayContext, parameter));

            return methodInfo.Invoke(shapeDriver, arguments.ToArray());
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter) {
            if (parameter.Name == "Shape")
                return displayContext.Value;

            if (parameter.Name == "Display")
                return displayContext.Display;

            var result = ((dynamic)(displayContext.Value))[parameter.Name];
            var converter = _converters.GetOrAdd(
                parameter.ParameterType,
                CompileConverter);
            return converter(result);
        }

        static Func<object, object> CompileConverter(Type targetType) {
            var valueParameter = Expression.Parameter(typeof (object), "value");

            return Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Dynamic(
                        Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, null),
                        targetType,
                        valueParameter),
                    typeof (object)),
                valueParameter).Compile();
        }

        static readonly ConcurrentDictionary<Type, Func<object, object>> _converters =
            new ConcurrentDictionary<Type, Func<object, object>>();

        static bool IsAcceptableMethod(MethodInfo methodInfo) {
            if (methodInfo.IsSpecialName)
                return false;
            if (methodInfo.DeclaringType == typeof(object))
                return false;
            return true;
        }
    }
}
