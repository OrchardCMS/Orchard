using System.IO;

namespace Orchard.Packaging {
    public interface IPackageManager : IDependency {
        PackageData Harvest(string extensionName);
        PackageData Download(string feedItemId);

        void Push(PackageData packageData, string feedUrl);
        PackageInfo Install(Stream packageStream);
    }
}