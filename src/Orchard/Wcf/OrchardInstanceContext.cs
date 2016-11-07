using System;
using System.Linq;
using System.ServiceModel;
using Autofac;
using Autofac.Core;

namespace Orchard.Wcf {
    public class OrchardInstanceContext : IExtension<InstanceContext>, IDisposable {
        private readonly IWorkContextScope _workContextScope;
        private readonly WorkContext _workContext;

        public OrchardInstanceContext(IWorkContextAccessor workContextAccessor) {
            _workContext = workContextAccessor.GetContext();

            if (_workContext == null) {
                _workContextScope = workContextAccessor.CreateWorkContextScope();
                _workContext = _workContextScope.WorkContext;
            }
        }

        public void Attach(InstanceContext owner) {}

        public void Detach(InstanceContext owner) {}

        public void Dispose() {
            if (_workContextScope != null) {
                _workContextScope.Dispose();
            }
        }

        public object Resolve(IComponentRegistration registration) {
            if (registration == null) {
                throw new ArgumentNullException("registration");
            }

            return _workContext.Resolve<ILifetimeScope>().ResolveComponent(registration, Enumerable.Empty<Parameter>());
        }
    }
}
