using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();
        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);

        void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle);
        void UninstallExtension(string extensionType, string extensionName);
    }

    public static class ExtensionManagerExtensions {
        public static IEnumerable<ExtensionDescriptor> EnabledExtensions(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            var enabledFeatures = EnabledFeatures(extensionManager, descriptor);
            return extensionManager.AvailableExtensions()
                .Where(extensionDescriptor =>
                    extensionDescriptor.Features.Any(featureDescriptor =>
                        enabledFeatures.Any(availableFeature => featureDescriptor.Name == availableFeature.Name)));
        }

        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableExtensions()
                .SelectMany(extensionDescriptor => extensionDescriptor.Features)
                .Where(featureDescriptor => IsFeatureEnabledInDescriptor(featureDescriptor, descriptor));
        }

        private static bool IsFeatureEnabledInDescriptor(FeatureDescriptor featureDescriptor, ShellDescriptor shellDescriptor) {
            return shellDescriptor.Features.Any(shellDescriptorFeature => shellDescriptorFeature.Name == featureDescriptor.Name);
        }
    }
}
