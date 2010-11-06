using System.IO;
using Orchard.Environment.Extensions;

namespace Orchard.Packaging.Services {
    public class PackageInfo {
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionPath { get; set; }
    }

    public interface IPackageExpander : IDependency {
        PackageInfo ExpandPackage(string packageId, string version, string location, string destination);
    }
}