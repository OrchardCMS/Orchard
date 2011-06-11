using System;
using System.IO;
using System.Linq;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Models;

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

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private PackageInfo DoInstall(Func<PackageInfo> installer) {
            try {
                return installer();
            }
            catch (Exception exception) {
                var message = T(
                    "There was an error installing the requested package. " +
                    "This can happen if the server does not have write access to the '~/Modules' or '~/Themes' folder of the web site. " +
                    "If the site is running in shared hosted environement, adding write access to these folders sometimes needs to be done manually through the Hoster control panel. " +
                    "Once Themes and Modules have been installed, it is recommended to remove write access to these folders.");
                throw new OrchardException(message, exception);
            }
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

        public PackageInfo Install(IPackage package, string location, string applicationPath) {
            return DoInstall(() => _packageExpander.Install(package, location, applicationPath));
        }

        public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
            return DoInstall(() => _packageExpander.Install(packageId, version, location, applicationPath));
        }

        public void Uninstall(string packageId, string applicationPath) {
            _packageExpander.Uninstall(packageId, applicationPath);
        }

        public ExtensionDescriptor GetExtensionDescriptor(IPackage package, string extensionType) {
            IPackageFile packageFile = package.GetFiles().FirstOrDefault(file =>
                                                            Path.GetFileName(file.Path).Equals(
                                                                DefaultExtensionTypes.IsModule(extensionType) ? "module.txt" : "theme.txt",
                                                                StringComparison.OrdinalIgnoreCase));

            if (packageFile != null) {
                string extensionId = Path.GetFileName(Path.GetDirectoryName(packageFile.Path).TrimEnd('/', '\\'));
                using (StreamReader streamReader = new StreamReader(packageFile.GetStream())) {
                    return ExtensionHarvester.GetDescriptorForExtension("", extensionId, extensionType, streamReader.ReadToEnd());
                }
            }

            return null;
        }

        #endregion
    }
}