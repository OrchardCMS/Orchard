using NuGet;
using Orchard.Environment.Extensions.Models;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    public interface IPackageManager : IDependency {
        PackageData Harvest(string extensionName);
        PackageInfo Install(IPackage package, string location, string applicationPath);
        PackageInfo Install(string packageId, string version, string location, string applicationPath);
        void Uninstall(string packageId, string applicationPath);
    }
}