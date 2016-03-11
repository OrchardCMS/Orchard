using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Autofac;

namespace Orchard.WebApi {
    public class AutofacWebApiDependencyResolver : IDependencyResolver {
        readonly ILifetimeScope _container;
        readonly IDependencyScope _rootDependencyScope;

        //internal static readonly string ApiRequestTag = "AutofacWebRequest";

        public AutofacWebApiDependencyResolver(ILifetimeScope container) {
            if (container == null) throw new ArgumentNullException("container");

            _container = container;
            _rootDependencyScope = new AutofacWebApiDependencyScope(container);
        }

        public ILifetimeScope Container {
            get { return _container; }
        }

        public object GetService(Type serviceType) {
            return _rootDependencyScope.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType) {
            return _rootDependencyScope.GetServices(serviceType);
        }

        public IDependencyScope BeginScope() {
            ILifetimeScope lifetimeScope = _container.BeginLifetimeScope();
            return new AutofacWebApiDependencyScope(lifetimeScope);
        }

        public void Dispose() {
            _rootDependencyScope.Dispose();
        }
    }
}
