using System.Collections.Generic;
using System.Web;

namespace Orchard.Modules {
    public interface IModuleService : IDependency {
        IModule GetModuleByName(string moduleName);
        IEnumerable<IModule> GetInstalledModules();
        void InstallModule(HttpPostedFileBase file);
        void UninstallModule(string moduleName);
        IEnumerable<IModuleFeature> GetAvailableFeatures();
        void EnableFeatures(IEnumerable<string> featureNames);
        void EnableFeatures(IEnumerable<string> featureNames, bool force);
        void DisableFeatures(IEnumerable<string> featureNames);
        void DisableFeatures(IEnumerable<string> featureNames, bool force);
    }
}