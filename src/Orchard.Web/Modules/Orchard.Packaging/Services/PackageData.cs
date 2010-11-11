using System.IO;

namespace Orchard.Packaging.Services {
    public class PackageData {
        public string ExtensionType { get; set; }
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }

        public Stream PackageStream { get; set; }
    }
}