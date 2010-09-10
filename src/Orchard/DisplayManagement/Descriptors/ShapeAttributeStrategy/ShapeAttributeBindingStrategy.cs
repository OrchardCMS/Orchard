using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using ClaySharp.Implementation;
using Microsoft.CSharp.RuntimeBinder;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Spooling;

namespace Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy {
    public class ShapeAttributeBindingStrategy : IShapeDescriptorBindingStrategy {
        private readonly IEnumerable<ShapeAttributeOccurrence> _shapeAttributeOccurrences;
        private readonly IComponentContext _componentContext;
        private readonly RouteCollection _routeCollection;

        public ShapeAttributeBindingStrategy(
            IEnumerable<ShapeAttributeOccurrence> shapeAttributeOccurrences,
            IComponentContext componentContext,
            RouteCollection routeCollection) {
            _shapeAttributeOccurrences = shapeAttributeOccurrences;
            // todo: using a component context won't work when this is singleton
            _componentContext = componentContext;
            _routeCollection = routeCollection;
        }

        public void Discover(ShapeTableBuilder builder) {
            foreach (var iter in _shapeAttributeOccurrences) {
                var occurrence = iter;
                var shapeType = occurrence.ShapeAttribute.ShapeType ?? occurrence.MethodInfo.Name;
                builder.Describe
                    .Named(shapeType)
                    .From(occurrence.Feature.Descriptor)
                    .BoundAs(
                        occurrence.MethodInfo.DeclaringType.FullName + "::" + occurrence.MethodInfo.Name,
                        descriptor => CreateDelegate(occurrence, descriptor));
            }
        }

        private Func<DisplayContext, IHtmlString> CreateDelegate(
            ShapeAttributeOccurrence attributeOccurrence,
            ShapeDescriptor descriptor) {
            return context => {
                var serviceInstance = _componentContext.Resolve(attributeOccurrence.Registration, Enumerable.Empty<Parameter>());

                // oversimplification for the sake of evolving
                return PerformInvoke(context, attributeOccurrence.MethodInfo, serviceInstance);
            };
        }


        private IHtmlString PerformInvoke(DisplayContext displayContext, MethodInfo methodInfo, object serviceInstance) {
            var output = new HtmlStringWriter();
            var arguments = methodInfo.GetParameters()
                .Select(parameter => BindParameter(displayContext, parameter, output));

            var returnValue = methodInfo.Invoke(serviceInstance, arguments.ToArray());
            if (methodInfo.ReturnType != typeof(void)) {
                output.Write(CoerceHtmlString(returnValue));
            }
            return output;
        }

        private static IHtmlString CoerceHtmlString(object invoke) {
            return invoke as IHtmlString ?? (invoke != null ? new HtmlString(invoke.ToString()) : null);
        }

        private object BindParameter(DisplayContext displayContext, ParameterInfo parameter, TextWriter output) {
            if (parameter.Name == "Shape")
                return displayContext.Value;

            if (parameter.Name == "Display")
                return displayContext.Display;

            if (parameter.Name == "Output" && parameter.ParameterType == typeof(TextWriter))
                return output;

            // meh--
            if (parameter.Name == "Html") {
                return new HtmlHelper(
                    displayContext.ViewContext,
                    displayContext.ViewDataContainer,
                    _routeCollection);
            }

            var getter = _getters.GetOrAdd(parameter.Name, n =>
                CallSite<Func<CallSite, object, dynamic>>.Create(
                Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                CSharpBinderFlags.None, n, null, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) })));

            var result = getter.Target(getter, displayContext.Value);

            //var result = ((dynamic)(displayContext.Value))[parameter.Name];
            if (result == null)
                return null;

            //if (parameter.Name == "Attributes") {
            //    var attributes = new RouteValueDictionary(result);
            //    return Arguments.From(attributes.Values, attributes.Keys);
            //}

            var converter = _converters.GetOrAdd(parameter.ParameterType, CompileConverter);
            var argument = converter.Invoke((object)result);
            return argument;
        }


        static readonly ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>> _getters =
            new ConcurrentDictionary<string, CallSite<Func<CallSite, object, dynamic>>>();

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