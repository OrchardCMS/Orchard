using System;
using System.IO;

namespace Orchard.Packaging {
    public class PackageInfo {
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }
        public string ExtensionType { get; set; }
        public string ExtensionPath { get; set; }
    }

    public interface IPackageExpander : IDependency {
        PackageInfo ExpandPackage(Stream packageStream);
    }
}