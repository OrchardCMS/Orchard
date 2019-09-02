using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Descriptor.Models;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        ExtensionDescriptor GetExtension(string id);

        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);
    }

    public static class ExtensionManagerExtensions {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return EnabledFeatures(extensionManager, descriptor.Features);
        }

        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, IEnumerable<ShellFeature> features) {
            return extensionManager.AvailableFeatures().Where(fd => features.Any(sf => sf.Name == fd.Id));
        }

        public static IEnumerable<FeatureDescriptor> DisabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableFeatures().Where(fd => descriptor.Features.All(sf => sf.Name != fd.Id));
        }
    }
}
