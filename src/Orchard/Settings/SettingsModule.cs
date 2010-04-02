using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using Orchard.Security;
using Module = Autofac.Module;

namespace Orchard.Settings {
    //public class SettingsModule : Module, IComponentInterceptorProvider {
    //    public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor) {
    //        var property = FindProperty(descriptor.BestKnownImplementationType);
    //        if (property != null) {
    //            if (property.GetGetMethod(true).IsVirtual == false) {
    //                throw new ApplicationException(string.Format("CurrentSite property must be virtual on class {0}", descriptor.BestKnownImplementationType.FullName));
    //            }
    //            yield return new TypedService(typeof(ISettingsModuleInterceptor));
    //        }
    //    }

    //    private static PropertyInfo FindProperty(Type type) {
    //        return type.GetProperty("CurrentSite",
    //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
    //            null,
    //            typeof(ISite),
    //            new Type[0],
    //            null);
    //    }        
    //}

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
