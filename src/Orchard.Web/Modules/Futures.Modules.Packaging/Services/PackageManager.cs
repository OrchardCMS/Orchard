using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using Orchard.Environment.Extensions;

namespace Futures.Modules.Packaging.Services {
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageSourceManager _packageSourceManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageExpander _packageExpander;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackageSourceManager packageSourceManager,
            IPackageBuilder packageBuilder,
            IPackageExpander packageExpander) {
            _extensionManager = extensionManager;
            _packageSourceManager = packageSourceManager;
            _packageBuilder = packageBuilder;
            _packageExpander = packageExpander;
        }

        public PackageData Harvest(string extensionName) {
            var extensionDescriptor = _extensionManager.AvailableExtensions().FirstOrDefault(x => x.Name == extensionName);
            if (extensionDescriptor == null)
                return null;
            return new PackageData {
                ExtensionName = extensionDescriptor.Name,
                ExtensionVersion = extensionDescriptor.Version,
                PackageStream = _packageBuilder.BuildPackage(extensionDescriptor),
            };
        }

        public void Push(PackageData packageData, string feedUrl) {

            var request = WebRequest.Create(feedUrl);
            request.Method = "POST";
            request.ContentType = "application/x-package";
            using (var requestStream = request.GetRequestStream()) {
                packageData.PackageStream.Seek(0, SeekOrigin.Begin);
                packageData.PackageStream.CopyTo(requestStream);
            }

            using (request.GetResponse()) {
                // forces request and disposes results
            }
        }

        public PackageData Download(string feedItemId) {
            var entry = _packageSourceManager.GetModuleList().Single(x => x.SyndicationItem.Id == feedItemId);
            var request = WebRequest.Create(entry.PackageStreamUri);
            using (var response = request.GetResponse()) {
                using (var responseStream = response.GetResponseStream()) {
                    var stream = new MemoryStream();
                    responseStream.CopyTo(stream);
                    var package = Package.Open(stream);
                    try {
                        return new PackageData {
                            ExtensionName = package.PackageProperties.Identifier,
                            ExtensionVersion = package.PackageProperties.Version,
                            PackageStream = stream
                        };
                    }
                    finally {
                        package.Close();
                    }
                }
            }
        }

        public void Install(PackageData packageData) {
            _packageExpander.ExpandPackage(packageData.PackageStream);
        }
    }
}
