using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageExpander _packageExpander;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackageBuilder packageBuilder,
            IPackageExpander packageExpander) {
            _extensionManager = extensionManager;
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
                ExtensionType = extensionDescriptor.ExtensionType,
                ExtensionName = extensionDescriptor.Name,
                ExtensionVersion = extensionDescriptor.Version,
                PackageStream = _packageBuilder.BuildPackage(extensionDescriptor),
            };
        }

        public PackageInfo Install(string packageId, string version, string location, string solutionFolder) {
            return _packageExpander.ExpandPackage(packageId, version, location, solutionFolder);
        }

        #endregion
    }
}