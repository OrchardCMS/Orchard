using System;
using System.IO;

namespace Orchard.Packaging.Services {
    public interface IPackageManager : IDependency {
        PackageData Harvest(string extensionName);
        PackageData Download(string feedItemId);

        void Push(PackageData packageData, string feedUrl, string login, string password);
        PackageInfo Install(string filename, string destination);
        PackageInfo Install(Uri uri, string destination);
    }
}