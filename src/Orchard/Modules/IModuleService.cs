using System.Collections.Generic;
using System.Web;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules {
    public interface IModuleService : IDependency {
        IModule GetModuleByName(string moduleName);
        IEnumerable<IModule> GetInstalledModules();
        void InstallModule(HttpPostedFileBase file);
        void UninstallModule(string moduleName);
        IEnumerable<Feature> GetAvailableFeatures();
        void EnableFeatures(IEnumerable<string> featureNames);
        void DisableFeatures(IEnumerable<string> featureNames);
    }
}