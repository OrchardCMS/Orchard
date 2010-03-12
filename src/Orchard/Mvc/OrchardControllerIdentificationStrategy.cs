using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Integration.Web.Mvc;
using Orchard.Extensions;

namespace Orchard.Mvc {
    public class OrchardControllerIdentificationStrategy : IControllerIdentificationStrategy {
        private readonly IEnumerable<ExtensionEntry> _extensions;

        public OrchardControllerIdentificationStrategy(IEnumerable<ExtensionEntry> extensions) {
            _extensions = extensions;
        }

        public Service ServiceForControllerName(string controllerName) {
            // the OrchardControllerFactory class does not call on the strategy, because the controller name
            // alone is insufficient to identify the service
            throw new NotImplementedException();
        }

        public Service ServiceForControllerType(Type controllerType) {
            var controllerNamespace = controllerType.Namespace;
            var extension = _extensions.FirstOrDefault(x => x.Assembly == controllerType.Assembly);
            var assemblySimpleName = controllerType.Assembly.GetName().Name;
            string areaName;
            if (assemblySimpleName == "Orchard.Core" && 
                controllerNamespace.StartsWith("Orchard.Core.")) {

                areaName = controllerNamespace.Split('.').Skip(2).FirstOrDefault();
            }
            else if (assemblySimpleName == "Orchard.Web" &&
                controllerNamespace.StartsWith("Orchard.Web.Areas.")) {

                areaName = controllerNamespace.Split('.').Skip(3).FirstOrDefault();
            }
            else if (extension != null) {
                areaName = extension.Descriptor.Name;
            }
            else {
                areaName = assemblySimpleName;
            }
            var controllerName = controllerType.Name.Replace("Controller", "");
            return new NamedService(("controller." + areaName + "." + controllerName).ToLowerInvariant());
        }
    }
}