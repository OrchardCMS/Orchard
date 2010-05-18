using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Localization;
using Orchard.Modules.Models;
using Orchard.UI.Notify;

namespace Orchard.Modules.Services {
    public class ModuleService : IModuleService {
        private const string ModuleExtensionType = "module";
        private readonly IExtensionManager _extensionManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public ModuleService(IOrchardServices orchardServices,IExtensionManager extensionManager, IShellDescriptorManager shellDescriptorManager) {
            Services = orchardServices;
            _extensionManager = extensionManager;
            _shellDescriptorManager = shellDescriptorManager;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

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
            var enabledFeatures = shellDescriptor.EnabledFeatures.ToList();
            var features = GetAvailableFeatures().ToList();

            foreach (var name in featureNames) {
                var featureName = name;
                var feature = features.Single(f => f.Descriptor.Name == featureName);
                var sleepingDependencies =
                    feature.Descriptor.Dependencies.Where(s => enabledFeatures.FirstOrDefault(sf => sf.Name == s) == null);

                if (sleepingDependencies.Count() != 0) {
                    Services.Notifier.Warning(T(
                        "If you want to enable {0}, then you'll also need {1} (and I won't let you flip everything on in one go yet).",
                        featureName,
                        sleepingDependencies.Count() > 1
                            ? string.Join("",
                                          sleepingDependencies.Select(
                                              (s, i) =>
                                              i == sleepingDependencies.Count() - 2
                                                  ? T("{0} and ", s).ToString()
                                                  : T("{0}, ", s).ToString()).ToArray())
                            : sleepingDependencies.First()));
                } else if (enabledFeatures.FirstOrDefault(f => f.Name == featureName) == null) {
                    enabledFeatures.Add(new ShellFeature {Name = featureName});
                    Services.Notifier.Information(T("{0} was enabled", featureName));
                }
            }

            _shellDescriptorManager.UpdateShellDescriptor(shellDescriptor.SerialNumber, enabledFeatures, shellDescriptor.Parameters);
        }

        public void DisableFeatures(IEnumerable<string> featureNames) {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            var enabledFeatures = shellDescriptor.EnabledFeatures.ToList();
            var features = GetAvailableFeatures().ToList();

            foreach (var name in featureNames) {
                var featureName = name;
                var dependants = features.Where(f => f.IsEnabled && f.Descriptor.Dependencies != null && f.Descriptor.Dependencies.Contains(featureName));

                if (dependants.Count() != 0) {
                    Services.Notifier.Warning(T(
                        "If you want to disable {0}, then you'll also lose {1} (and I won't let you do that yet).",
                        featureName,
                        dependants.Count() > 0
                            ? string.Join("",
                                          dependants.Select(
                                              (f, i) =>
                                              i == dependants.Count() - 2
                                                  ? T("{0} and ", f.Descriptor.Name).ToString()
                                                  : T("{0}, ", f.Descriptor.Name).ToString()).ToArray())
                            : dependants.First().Descriptor.Name));
                }
                else {
                    enabledFeatures.RemoveAll(f => f.Name == featureName);
                    Services.Notifier.Information(T("{0} was disabled", featureName));
                }
            }

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