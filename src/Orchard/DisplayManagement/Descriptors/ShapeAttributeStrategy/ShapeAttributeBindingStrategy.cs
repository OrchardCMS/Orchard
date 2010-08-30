using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy {
    public class ShapeAttributeBindingStrategy : IShapeDescriptorBindingStrategy {
        private readonly IEnumerable<ShapeAttributeOccurrence> _shapeAttributeOccurrences;
        private readonly IComponentContext _componentContext;        

        public ShapeAttributeBindingStrategy(
            IEnumerable<ShapeAttributeOccurrence> shapeAttributeOccurrences,
            IComponentContext componentContext) {
            _shapeAttributeOccurrences = shapeAttributeOccurrences;
            // todo: using a component context won't work when this is singleton
            _componentContext = componentContext;
        }

        public void Discover(ShapeTableBuilder builder) {
            foreach (var iter in _shapeAttributeOccurrences) {
                var occurrence = iter;
                var shapeType = occurrence.ShapeAttribute.ShapeType ?? occurrence.MethodInfo.Name;
                builder.Describe
                    .Named(shapeType)
                    .From(occurrence.Feature)
                    .BoundAs(descriptor => CreateDelegate(occurrence, descriptor));
            }
        }

        private ShapeBinding CreateDelegate(
            ShapeAttributeOccurrence attributeOccurrence,
            ShapeDescriptor descriptor) {
            return context => {
                var serviceInstance = _componentContext.Resolve(attributeOccurrence.Registration, Enumerable.Empty<Parameter>());
                
                // oversimplification for the sake of evolving
                return PerformInvoke(context, attributeOccurrence.MethodInfo, serviceInstance);
            };
        }


        private object PerformInvoke(DisplayContext displayContext, MethodInfo methodInfo, object serviceInstance) {
            var arguments = methodInfo.GetParameters()
                .Select(parameter => BindParameter(displayContext, parameter));

            return methodInfo.Invoke(serviceInstance, arguments.ToArray());
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter) {
            if (parameter.Name == "Shape")
                return displayContext.Value;

            if (parameter.Name == "Display")
                return displayContext.Display;

            var result = ((dynamic)(displayContext.Value))[parameter.Name];
            var converter = _converters.GetOrAdd(parameter.ParameterType, CompileConverter);
            return converter.Invoke((object)result);
        }

        static readonly ConcurrentDictionary<Type, Func<object, object>> _converters =
            new ConcurrentDictionary<Type, Func<object, object>>();

        static Func<object, object> CompileConverter(Type targetType) {
            var valueParameter = Expression.Parameter(typeof(object), "value");

            return Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Dynamic(
                        Microsoft.CSharp.RuntimeBinder.Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, null),
                        targetType,
                        valueParameter),
                    typeof(object)),
                valueParameter).Compile();
        }
    }
}