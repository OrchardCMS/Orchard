using System.Collections.Generic;
using System.Web;
using Orchard.Extensions.Models;

namespace Orchard.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<ExtensionEntry> ActiveExtensions();
        ShellTopology_Obsolete GetExtensionsTopology();
        Feature LoadFeature(string featureName);
        void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle);
        void UninstallExtension(string extensionType, string extensionName);
    }
}