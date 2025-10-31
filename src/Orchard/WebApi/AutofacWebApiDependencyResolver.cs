using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Autofac;

namespace Orchard.WebApi
{
    public class AutofacWebApiDependencyResolver : IDependencyResolver
    {
        readonly IDependencyScope _rootDependencyScope;

        //internal static readonly string ApiRequestTag = "AutofacWebRequest";

        public AutofacWebApiDependencyResolver(ILifetimeScope container)
        {
            if (container == null) throw new ArgumentNullException("container");

            Container = container;
            _rootDependencyScope = new AutofacWebApiDependencyScope(container);
        }

        public ILifetimeScope Container { get; }

        public object GetService(Type serviceType)
        {
            return _rootDependencyScope.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _rootDependencyScope.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            ILifetimeScope lifetimeScope = Container.BeginLifetimeScope();
            return new AutofacWebApiDependencyScope(lifetimeScope);
        }

        public void Dispose()
        {
            _rootDependencyScope.Dispose();
        }
    }
}
