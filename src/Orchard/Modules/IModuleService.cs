using System.Collections.Generic;
using System.Web;

namespace Orchard.Modules {
    public interface IModuleService : IDependency {
        IModule GetModuleByName(string moduleName);
        IEnumerable<IModule> GetInstalledModules();
        void InstallModule(HttpPostedFileBase file);
        void UninstallModule(string moduleName);
    }
}