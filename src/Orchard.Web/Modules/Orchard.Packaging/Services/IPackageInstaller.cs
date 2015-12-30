using NuGet;

namespace Orchard.Packaging.Services {
    public class PackageInfo {
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionPath { get; set; }
    }

    public interface IPackageInstaller : IDependency {
        PackageInfo Install(IPackage package, string location, string applicationPath);
        PackageInfo Install(string packageId, string version, string location, string applicationFolder);
        void Uninstall(string packageId, string applicationFolder);
    }
}