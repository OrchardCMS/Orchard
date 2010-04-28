using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Modules.Models;

namespace Orchard.Modules.Services {
    public class ModuleService : IModuleService {
        private const string ModuleExtensionType = "module";
        private readonly IExtensionManager _extensionManager;

        public ModuleService(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public IModule GetModuleByName(string moduleName) {
            return _extensionManager.AvailableExtensions().Where(e => string.Equals(e.Name, moduleName, StringComparison.OrdinalIgnoreCase) && string.Equals(e.ExtensionType, "Module", StringComparison.OrdinalIgnoreCase)).Select(
                descriptor => AssembleModuleFromDescriptor(descriptor)).FirstOrDefault();
        }

        public IEnumerable<IModule> GetInstalledModules() {
            return
                _extensionManager.AvailableExtensions().Where(
                    e => String.Equals(e.ExtensionType, ModuleExtensionType, StringComparison.OrdinalIgnoreCase)).Select(
                    descriptor => AssembleModuleFromDescriptor(descriptor));
        }

        public void InstallModule(HttpPostedFileBase file) {
            _extensionManager.InstallExtension(ModuleExtensionType, file);
        }

        public void UninstallModule(string moduleName) {
            _extensionManager.UninstallExtension(ModuleExtensionType, moduleName);
        }

        public IEnumerable<Feature> GetAvailableFeatures() {
            return GetInstalledModules()
                .Where(m => m.Features != null)
                .SelectMany(m => _extensionManager.LoadFeatures(m.Features));
        }

        public IEnumerable<Feature> GetAvailableFeaturesByModule(string moduleName) {
            var module = GetModuleByName(moduleName);
            if (module == null || module.Features == null)
                return null;

            return _extensionManager.LoadFeatures(module.Features);
        }

        public void EnableFeatures(IEnumerable<string> featureNames) {
        }

        public void DisableFeatures(IEnumerable<string> featureNames) {
        }

        private static IModule AssembleModuleFromDescriptor(ExtensionDescriptor extensionDescriptor) {
            return new Module {
                                  ModuleName = extensionDescriptor.Name,
                                  DisplayName = extensionDescriptor.DisplayName,
                                  Description = extensionDescriptor.Description,
                                  Version = extensionDescriptor.Version,
                                  Author = extensionDescriptor.Author,
                                  HomePage = extensionDescriptor.WebSite,
                                  Tags = extensionDescriptor.Tags,
                                  Features = extensionDescriptor.Features
                              };
        }
    }
}