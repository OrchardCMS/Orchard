using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Modules.Models;

namespace Orchard.Modules.Services {
    public class ModuleService : IModuleService {
        private readonly IExtensionManager _extensionManager;

        public ModuleService(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public IModule GetModuleByName(string moduleName) {
            return null;
        }

        public IEnumerable<IModule> GetInstalledModules() {
            return
                _extensionManager.AvailableExtensions().Where(
                    e => String.Equals(e.ExtensionType, "Module", StringComparison.OrdinalIgnoreCase)).Select(
                    descriptor => (new Module {
                                                  ModuleName = descriptor.Name,
                                                  DisplayName = descriptor.DisplayName,
                                                  Description = descriptor.Description,
                                                  Version = descriptor.Version,
                                                  Author = descriptor.Author,
                                                  HomePage = descriptor.WebSite,
                                                  Tags = descriptor.Tags
                                              }) as IModule);
        }

        public void InstallModule(HttpPostedFileBase file) {
        }

        public void UninstallModule(string moduleName) {
        }
    }
}