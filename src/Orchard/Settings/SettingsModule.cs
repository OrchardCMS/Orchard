using System;
using System.Reflection;
using Autofac.Core;
using Castle.Core.Interceptor;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Module = Autofac.Module;

namespace Orchard.Settings {
    public class SettingsModule : Module {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
            var implementationType = registration.Activator.LimitType;
            var property = FindProperty(implementationType);

            if (property != null) {
                registration.InterceptedBy<ISettingsModuleInterceptor>();
            }
        }

        private static PropertyInfo FindProperty(Type type) {
            return type.GetProperty("CurrentSite",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                typeof(ISite),
                new Type[0],
                null);
        }
    }

    public interface ISettingsModuleInterceptor : IInterceptor, IDependency {

    }

    public class SettingsModuleInterceptor : ISettingsModuleInterceptor {
        private readonly ISiteService _siteService;

        public SettingsModuleInterceptor(ISiteService siteService) {
            _siteService = siteService;
        }

        public void Intercept(IInvocation invocation) {
            if (invocation.Method.ReturnType == typeof(ISite) && invocation.Method.Name == "get_CurrentSite") {
                invocation.ReturnValue = _siteService.GetSiteSettings();
            }
            else {
                invocation.Proceed();
            }
        }
    }
}
