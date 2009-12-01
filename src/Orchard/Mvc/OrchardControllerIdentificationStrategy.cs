using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Integration.Web.Mvc;
using Orchard.Packages;

namespace Orchard.Mvc {
    public class OrchardControllerIdentificationStrategy : IControllerIdentificationStrategy {
        private readonly IEnumerable<PackageEntry> _packages;

        public OrchardControllerIdentificationStrategy(IEnumerable<PackageEntry> packages) {
            _packages = packages;
        }

        public Service ServiceForControllerName(string controllerName) {
            // the OrchardControllerFactory class does not call on the strategy, because the controller name
            // alone is insufficient to identify the service
            throw new NotImplementedException();
        }

        public Service ServiceForControllerType(Type controllerType) {
            var controllerNamespace = controllerType.Namespace;
            var package = _packages.FirstOrDefault(x => x.Assembly == controllerType.Assembly);
            var assemblySimpleName = controllerType.Assembly.GetName().Name;
            string areaName;
            if (assemblySimpleName == "Orchard.Core" && 
                controllerNamespace.StartsWith("Orchard.Core.")) {

                areaName = controllerNamespace.Split('.').Skip(2).FirstOrDefault();
            }
            else if (package != null) {
                areaName = package.Descriptor.Name;
            }
            else {
                areaName = assemblySimpleName;
            }
            var controllerName = controllerType.Name.Replace("Controller", "");
            return new NamedService(("controller." + areaName + "." + controllerName).ToLowerInvariant());
        }
    }
}