using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Castle.Core.Logging;
using Module = Autofac.Builder.Module;

namespace Orchard.Logging {

    public class LoggingModule : Module {
        protected override void Load(ContainerBuilder moduleBuilder) {
            // by default, use Orchard's logger that delegates to Castle's logger factory
            moduleBuilder.Register<CastleLoggerFactory>().As<ILoggerFactory>().ContainerScoped();

            // by default, use Castle's TraceSource based logger factory
            moduleBuilder.Register<TraceLoggerFactory>().As<Castle.Core.Logging.ILoggerFactory>().ContainerScoped();

            // call CreateLogger in response to the request for an ILogger implementation
            moduleBuilder.Register((ctx, ps) => CreateLogger(ctx, ps)).As<ILogger>().FactoryScoped();
        }

        protected override void AttachToComponentRegistration(IContainer container, IComponentRegistration registration) {
            var implementationType = registration.Descriptor.BestKnownImplementationType;

            // build an array of actions on this type to assign loggers to member properties
            var injectors = BuildLoggerInjectors(implementationType).ToArray();

            // if there are no logger properties, there's no reason to hook the activated event
            if (!injectors.Any())
                return;

            // otherwise, whan an instance of this component is activated, inject the loggers on the instance
            registration.Activated += (s, e) => {
                foreach (var injector in injectors)
                    injector(e.Context, e.Instance);
            };
        }

        private IEnumerable<Action<IContext, object>> BuildLoggerInjectors(Type componentType) {
            // Look for settable properties of type "ILogger" 
            var loggerProperties = componentType
                .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new {
                    PropertyInfo = p,
                    p.PropertyType,
                    IndexParameters = p.GetIndexParameters(),
                    Accessors = p.GetAccessors(false)
                })
                .Where(x => x.PropertyType == typeof(ILogger)) // must be a logger
                .Where(x => x.IndexParameters.Count() == 0) // must not be an indexer
                .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)); //must have get/set, or only set

            // Return an array of actions that resolve a logger and assign the property
            foreach (var entry in loggerProperties) {
                var propertyInfo = entry.PropertyInfo;

                yield return (ctx, instance) => {
                    var propertyValue = ctx.Resolve<ILogger>(new TypedParameter(typeof(Type), componentType));
                    propertyInfo.SetValue(instance, propertyValue, null);
                };
            }
        }

        private static ILogger CreateLogger(IContext context, IEnumerable<Parameter> parameters) {
            // return an ILogger in response to Resolve<ILogger>(componentTypeParameter)
            var loggerFactory = context.Resolve<ILoggerFactory>();
            var containingType = parameters.TypedAs<Type>();
            return loggerFactory.CreateLogger(containingType);
        }
    }
}
