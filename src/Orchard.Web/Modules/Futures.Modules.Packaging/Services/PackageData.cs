using System.IO;

namespace Futures.Modules.Packaging.Services {
    public class PackageData {
        public string ExtensionName { get; set; }
        public string ExtensionVersion { get; set; }

        public Stream PackageStream { get; set; }
    }
}