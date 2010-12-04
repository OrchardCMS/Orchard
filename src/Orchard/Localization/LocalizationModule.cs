using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Module = Autofac.Module;

namespace Orchard.Localization {
    public class LocalizationModule : Module {
        private readonly IDictionary<string, Localizer> _localizerCache;
        
        public LocalizationModule() {
            _localizerCache = new ConcurrentDictionary<string, Localizer>();
        }

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<Text>().As<IText>().InstancePerDependency();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            var userProperty = FindUserProperty(registration.Activator.LimitType);

            if (userProperty != null) {
                var scope = registration.Activator.LimitType.FullName;

                registration.Activated += (sender, e) => {
                    if (_localizerCache.ContainsKey(scope)) {
                        userProperty.SetValue(e.Instance, _localizerCache[scope], null);
                    }
                    else                    {
                        var localizer = LocalizationUtilities.Resolve(e.Context, scope);
                        _localizerCache.Add(scope, localizer);
                        userProperty.SetValue(e.Instance, localizer, null);
                    }
                };
            }
        }

        private static PropertyInfo FindUserProperty(Type type) {
            return type.GetProperty("T", typeof(Localizer));
        }
    }
}
