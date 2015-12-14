using System;
using System.Linq;
using Autofac;

namespace Orchard.Caching {
    public class CacheModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<DefaultCacheManager>()
                .As<ICacheManager>()
                .InstancePerDependency();
        }

        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration) {
            var needsCacheManager = registration.Activator.LimitType
                .GetConstructors()
                .Any(x => x.GetParameters()
                    .Any(xx => xx.ParameterType == typeof(ICacheManager)));

            if (needsCacheManager) {
                registration.Preparing += (sender, e) => {
                    var parameter = new TypedParameter(
                        typeof(ICacheManager),
                        e.Context.Resolve<ICacheManager>(new TypedParameter(typeof(Type), registration.Activator.LimitType)));
                    e.Parameters = e.Parameters.Concat(new[] { parameter });
                };
            }
        }
    }
}
