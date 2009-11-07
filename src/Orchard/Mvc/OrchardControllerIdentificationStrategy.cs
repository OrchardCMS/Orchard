using System;
using Autofac;
using Autofac.Integration.Web.Mvc;

namespace Orchard.Mvc {
    public class OrchardControllerIdentificationStrategy : IControllerIdentificationStrategy {
        public Service ServiceForControllerName(string controllerName) {
            // the OrchardControllerFactory class does not call on the strategy, because the controller name
            // alone is insufficient to identify the service
            throw new NotImplementedException();
        }

        public Service ServiceForControllerType(Type controllerType) {
            var assemblySimpleName = controllerType.Assembly.GetName().Name;
            var controllerName = controllerType.Name.Replace("Controller", "");
            return new NamedService(("controller." + assemblySimpleName + "." + controllerName).ToLowerInvariant());
        }
    }
}