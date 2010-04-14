using System;
using System.Collections.Generic;
using System.Web;

namespace Orchard.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<ExtensionEntry> ActiveExtensions();
        IEnumerable<Type> LoadFeature(string featureName);
        void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle);
        void UninstallExtension(string extensionType, string extensionName);
    }
}