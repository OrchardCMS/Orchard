namespace Orchard.Packaging.Services {
    public interface IPackageManager : IDependency {
        PackageData Harvest(string extensionName);
        PackageInfo Install(string packageId, string version, string location, string applicationPath);
        void Uninstall(string packageId, string applicationPath);
    }
}