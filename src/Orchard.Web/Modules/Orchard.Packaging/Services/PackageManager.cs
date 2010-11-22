using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageInstaller _packageExpander;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackageBuilder packageBuilder,
            IPackageInstaller packageExpander) {
            _extensionManager = extensionManager;
            _packageBuilder = packageBuilder;
            _packageExpander = packageExpander;
        }

        #region IPackageManager Members

        public PackageData Harvest(string extensionName) {
            ExtensionDescriptor extensionDescriptor = _extensionManager.AvailableExtensions().FirstOrDefault(x => x.Id == extensionName);
            if (extensionDescriptor == null) {
                return null;
            }
            return new PackageData {
                ExtensionType = extensionDescriptor.ExtensionType,
                ExtensionName = extensionDescriptor.Id,
                ExtensionVersion = extensionDescriptor.Version,
                PackageStream = _packageBuilder.BuildPackage(extensionDescriptor),
            };
        }

        public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
            return _packageExpander.Install(packageId, version, location, applicationPath);
        }

        public void Uninstall(string packageId, string applicationPath) {
            _packageExpander.Uninstall(packageId, applicationPath);
        }
        #endregion
    }
}