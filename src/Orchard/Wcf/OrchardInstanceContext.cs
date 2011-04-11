using System;
using System.Linq;
using System.ServiceModel;
using Autofac;
using Autofac.Core;

namespace Orchard.Wcf {
    public class OrchardInstanceContext : IExtension<InstanceContext>, IDisposable {
        private readonly IWorkContextScope _workContextScope;

        public OrchardInstanceContext(IWorkContextAccessor workContextAccessor) {
            _workContextScope = workContextAccessor.CreateWorkContextScope();
        }

        public void Attach(InstanceContext owner) {}

        public void Detach(InstanceContext owner) {}

        public void Dispose() {
            _workContextScope.Dispose();
        }

        public object Resolve(IComponentRegistration registration) {
            if (registration == null) {
                throw new ArgumentNullException("registration");
            }

            return _workContextScope.Resolve<ILifetimeScope>().Resolve(registration, Enumerable.Empty<Parameter>());
        }
    }
}
