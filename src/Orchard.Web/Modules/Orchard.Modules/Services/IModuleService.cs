using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.Services {
    public interface IModuleService : IDependency {
        void EnableFeatures(IEnumerable<string> featureNames);
        void EnableFeatures(IEnumerable<string> featureNames, bool force);
        void DisableFeatures(IEnumerable<string> featureNames);
        void DisableFeatures(IEnumerable<string> featureNames, bool force);
        bool UpdateIsRecentlyInstalled(ExtensionDescriptor module);
    }
}