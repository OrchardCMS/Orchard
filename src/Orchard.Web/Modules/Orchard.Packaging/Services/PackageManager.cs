using System;
using System.Linq;
using System.Web.Hosting;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageInstaller _packageExpander;
        private readonly INotifier _notifier;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackageBuilder packageBuilder,
            IPackageInstaller packageExpander,
            INotifier notifier) {
            _extensionManager = extensionManager;
            _packageBuilder = packageBuilder;
            _packageExpander = packageExpander;
            _notifier = notifier;

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

        #endregion
    }
}