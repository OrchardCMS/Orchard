using System;
using System.IO;

namespace Orchard.Packaging.Services {
    public interface IPackageManager : IDependency {
        PackageData Harvest(string extensionName);
        PackageInfo Install(string packageId, string version, string location, string solutionFolder);
    }
}