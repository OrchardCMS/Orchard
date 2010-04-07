using System;
using System.Reflection;
using Autofac.Core;
using Castle.Core.Interceptor;
using Orchard.Environment.AutofacUtil.DynamicProxy2;
using Module = Autofac.Module;

namespace Orchard.Security {
    public class SecurityModule : Module {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
            var implementationType = registration.Activator.LimitType;
            var property = FindProperty(implementationType);

            if (property != null) {
                registration.InterceptedBy<ISecurityModuleInterceptor>();
            }
        }

        private static PropertyInfo FindProperty(Type type) {
            return type.GetProperty("CurrentUser",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                typeof(IUser),
                new Type[0],
                null);
        }
    }

    public interface ISecurityModuleInterceptor : IInterceptor, IDependency {

    }

    public class SecurityModuleInterceptor : ISecurityModuleInterceptor {
        private readonly IAuthenticationService _authenticationService;

        public SecurityModuleInterceptor(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void Intercept(IInvocation invocation) {
            if (invocation.Method.ReturnType == typeof(IUser) && invocation.Method.Name == "get_CurrentUser") {
                invocation.ReturnValue = _authenticationService.GetAuthenticatedUser();
            }
            else {
                invocation.Proceed();
            }
        }
    }
}
