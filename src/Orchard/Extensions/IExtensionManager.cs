using System;
using System.Collections.Generic;
using System.Web;
using Orchard.Extensions.Models;

namespace Orchard.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<Type> LoadFeature(string featureName);
        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> features);

        IEnumerable<ExtensionEntry> ActiveExtensions();
        ShellTopology_Obsolete GetExtensionsTopology();

        void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle);
        void UninstallExtension(string extensionType, string extensionName);
    }
}