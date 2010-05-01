using System.Collections.Generic;
using System.Web;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules {
    public interface IModuleService : IDependency {
        IModule GetModuleByName(string moduleName);
        IEnumerable<IModule> GetInstalledModules();
        void InstallModule(HttpPostedFileBase file);
        void UninstallModule(string moduleName);
        IModule GetModuleByFeatureName(string featureName);
        IEnumerable<IModuleFeature> GetAvailableFeatures();
        void EnableFeatures(IEnumerable<string> featureNames);
        void DisableFeatures(IEnumerable<string> featureNames);
    }
}