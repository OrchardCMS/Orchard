using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Modules.Models;

namespace Orchard.Modules.Services {
    public class ModuleService : IModuleService {
        private const string ModuleExtensionType = "module";
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public ModuleService(IExtensionManager extensionManager, IShellDescriptorManager shellDescriptorManager) {
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public IModule GetModuleByName(string moduleName) {
            return _extensionManager.AvailableExtensions().Where(e => string.Equals(e.Name, moduleName, StringComparison.OrdinalIgnoreCase) && string.Equals(e.ExtensionType, ModuleExtensionType, StringComparison.OrdinalIgnoreCase)).Select(
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

        public IModule GetModuleByFeatureName(string featureName) {
            return GetInstalledModules()
                .Where(
                m =>
                m.Features.FirstOrDefault(f => string.Equals(f.Name, featureName, StringComparison.OrdinalIgnoreCase)) !=
                null).FirstOrDefault();
        }

        public IEnumerable<IModuleFeature> GetAvailableFeatures() {
            var enabledFeatures = _shellDescriptorManager.GetShellDescriptor().EnabledFeatures;
            return GetInstalledModules()
                .SelectMany(m => _extensionManager.LoadFeatures(m.Features))
                .Select(f => AssembleModuleFromDescriptor(f, enabledFeatures.FirstOrDefault(sf => string.Equals(sf.Name, f.Descriptor.Name, StringComparison.OrdinalIgnoreCase)) != null));
        }

        public IEnumerable<Feature> GetAvailableFeaturesByModule(string moduleName) {
            throw new NotImplementedException();
        }

        public void EnableFeatures(IEnumerable<string> featureNames) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();

            var enabledFeatures = shellDescriptor.EnabledFeatures
                .Union(featureNames.Select(s => new ShellFeature {Name = s}));

            _shellDescriptorManager.UpdateShellDescriptor(shellDescriptor.SerialNumber, enabledFeatures, shellDescriptor.Parameters);
        }

        public void DisableFeatures(IEnumerable<string> featureNames) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();

            var enabledFeatures = shellDescriptor.EnabledFeatures.ToList();
            enabledFeatures.RemoveAll(f => featureNames.Contains(f.Name));

            _shellDescriptorManager.UpdateShellDescriptor(shellDescriptor.SerialNumber, enabledFeatures, shellDescriptor.Parameters);
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

        private static IModuleFeature AssembleModuleFromDescriptor(Feature feature, bool isEnabled) {
            return new ModuleFeature {
                                         Descriptor = feature.Descriptor,
                                         IsEnabled = isEnabled
                                     };
        }
    }
}