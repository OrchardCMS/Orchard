using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        ExtensionDescriptor GetExtension(string id);

        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);
    }
}
