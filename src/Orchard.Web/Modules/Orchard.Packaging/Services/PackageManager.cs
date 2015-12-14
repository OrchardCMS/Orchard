using System;
using System.IO;
using System.Linq;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageManager : IPackageManager {
        private readonly IExtensionManager _extensionManager;
        private readonly IPackageBuilder _packageBuilder;
        private readonly IPackageInstaller _packageInstaller;
        private readonly IShellStateManager _shellStateManager;
        private readonly IFeatureManager _featureManager;
        private readonly IPackageUninstallHandler _packageUninstallHandler;

        public PackageManager(
            IExtensionManager extensionManager,
            IPackageBuilder packageBuilder,
            IPackageInstaller packageInstaller,
            IShellStateManager shellStateManager,
            IFeatureManager featureManager,
            IPackageUninstallHandler packageUninstallHandler) {
            _extensionManager = extensionManager;
            _packageBuilder = packageBuilder;
            _packageInstaller = packageInstaller;
            _shellStateManager = shellStateManager;
            _featureManager = featureManager;
            _packageUninstallHandler = packageUninstallHandler;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private PackageInfo DoInstall(Func<PackageInfo> installer) {
            try {
                return installer();
            }
            catch (OrchardException) {
                throw;
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
            return DoInstall(() => _packageInstaller.Install(package, location, applicationPath));
        }

        public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
            return DoInstall(() => _packageInstaller.Install(packageId, version, location, applicationPath));
        }

        public void Uninstall(string packageId, string applicationPath) {
            var extensionToUninstall = _extensionManager.AvailableExtensions()
                .FirstOrDefault(extension => PackageBuilder.BuildPackageId(extension.Id, extension.ExtensionType) == packageId);

            if (extensionToUninstall == null) {
                throw new OrchardException(T("There is no extension that has the package ID \"{0}\".", packageId));
            }

            var featureIdsToUninstall = extensionToUninstall.Features.Select(feature => feature.Id);
            var shellState = _shellStateManager.GetShellState();
            var featureStates = shellState.Features.Where(featureState => featureIdsToUninstall.Contains(featureState.Name));

            // This means that no feature from this extension wasn enabled yet, can be uninstalled directly.
            if (!featureStates.Any()) {
                _packageUninstallHandler.QueuePackageUninstall(packageId);
            }
            else {
                _featureManager.DisableFeatures(extensionToUninstall.Features.Select(feature => feature.Id), true);

                // Installed state can't be deduced from the shell state changes like for enabled state, so have to
                // set that explicitly.
                foreach (var featureState in featureStates) {
                    _shellStateManager.UpdateInstalledState(featureState, Environment.State.Models.ShellFeatureState.State.Falling);
                }
            }
        }

        #endregion
    }
}