using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageExpander _packageExpander;
        private readonly IPackagingSourceManager _packagingSourceManager;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackagingSourceManager packagingSourceManager,
            IPackageBuilder packageBuilder,
            IPackageExpander packageExpander) {
            _extensionManager = extensionManager;
            _packagingSourceManager = packagingSourceManager;
            _packageBuilder = packageBuilder;
            _packageExpander = packageExpander;
        }

        #region IPackageManager Members

        public PackageData Harvest(string extensionName) {
            ExtensionDescriptor extensionDescriptor = _extensionManager.AvailableExtensions().FirstOrDefault(x => x.Name == extensionName);
            if (extensionDescriptor == null) {
                return null;
            }
            return new PackageData {
                ExtensionName = extensionDescriptor.Name,
                ExtensionVersion = extensionDescriptor.Version,
                PackageStream = _packageBuilder.BuildPackage(extensionDescriptor),
            };
        }

        public void Push(PackageData packageData, string feedUrl) {
            WebRequest request = WebRequest.Create(feedUrl);
            request.Method = "POST";
            request.ContentType = "application/x-package";
            using (Stream requestStream = request.GetRequestStream()) {
                packageData.PackageStream.Seek(0, SeekOrigin.Begin);
                packageData.PackageStream.CopyTo(requestStream);
            }

            using (request.GetResponse()) {
                // forces request and disposes results
            }
        }

        public PackageData Download(string feedItemId) {
            PackagingEntry entry = _packagingSourceManager.GetModuleList().Single(x => x.SyndicationItem.Id == feedItemId);
            WebRequest request = WebRequest.Create(entry.PackageStreamUri);
            using (WebResponse response = request.GetResponse()) {
                using (Stream responseStream = response.GetResponseStream()) {
                    var stream = new MemoryStream();
                    responseStream.CopyTo(stream);
                    Package package = Package.Open(stream);
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

        public PackageInfo Install(Stream packageStream) {
            return _packageExpander.ExpandPackage(packageStream);
        }

        #endregion
    }
}