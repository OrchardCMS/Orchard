using System;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Orchard.Environment.AutofacUtil.DynamicProxy2 {
    public static class DynamicProxyExtensions {

        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData {

            dynamicProxyContext.EnableDynamicProxy(rb);

            return rb;
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TRegistrationStyle>(
           this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext) {

            rb.ActivatorData.ConfigurationActions.Add((t, rb2) => rb2.EnableDynamicProxy(dynamicProxyContext));
            return rb;
        }

        public static void InterceptedBy<TService>(this IComponentRegistration cr) {
            var dynamicProxyContext = DynamicProxyContext.From(cr);
            if (dynamicProxyContext == null)
                throw new ApplicationException(string.Format("Component {0} was not registered with EnableDynamicProxy", cr.Activator.LimitType));

            dynamicProxyContext.AddInterceptorService(cr, new TypedService(typeof(TService)));
        }
    }
}
