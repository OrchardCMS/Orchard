using System;
using System.Collections.Generic;

namespace Orchard.Modules {
    [Obsolete]
    public interface IModuleService : IDependency {
        [Obsolete]
        IEnumerable<IModule> GetInstalledModules();
        [Obsolete]
        IEnumerable<IModuleFeature> GetAvailableFeatures();
        [Obsolete]
        void EnableFeatures(IEnumerable<string> featureNames);
        [Obsolete]
        void EnableFeatures(IEnumerable<string> featureNames, bool force);
        [Obsolete]
        void DisableFeatures(IEnumerable<string> featureNames);
        [Obsolete]
        void DisableFeatures(IEnumerable<string> featureNames, bool force);
    }
}