using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using AutofacContrib.DynamicProxy2;
using Castle.Core.Interceptor;
using Module = Autofac.Builder.Module;

namespace Orchard.Security {
    public class SecurityModule : Module, IComponentInterceptorProvider {
        public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor) {
            var property = FindProperty(descriptor.BestKnownImplementationType);
            if (property != null) {
                if (property.GetGetMethod(true).IsVirtual == false) {
                    throw new ApplicationException(string.Format("CurrentUser property must be virtual on class {0}", descriptor.BestKnownImplementationType.FullName));
                }
                yield return new TypedService(typeof(ISecurityModuleInterceptor));
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
