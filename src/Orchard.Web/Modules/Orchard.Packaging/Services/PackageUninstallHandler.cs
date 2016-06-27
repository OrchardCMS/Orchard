using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.State;
using Orchard.Events;

namespace Orchard.Packaging.Services {
    public interface IPackageUninstallHandler : IEventHandler {
        /// <summary>
        /// Queues a package to be uninstalled after the request is processed.
        /// </summary>
        /// <param name="packageId">The textual ID of the package.</param>
        void QueuePackageUninstall(string packageId);

        /// <summary>
        /// Uninstalls the given package from the system.
        /// </summary>
        /// <param name="packageId">The textual ID of the package.</param>
        void UninstallPackage(string packageId);
    }

    public class PackageUninstallHandler : IFeatureEventHandler, IPackageUninstallHandler {
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IProcessingEngine _processingEngine;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IPackageInstaller _packageInstaller;

        public PackageUninstallHandler(
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IProcessingEngine processingEngine,
            IHostEnvironment hostEnvironment,
            IPackageInstaller packageInstaller) {
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _processingEngine = processingEngine;
            _hostEnvironment = hostEnvironment;
            _packageInstaller = packageInstaller;
        }

        public void Installing(Feature feature) {
        }

        public void Installed(Feature feature) {
        }

        public void Enabling(Feature feature) {
        }

        public void Enabled(Feature feature) {
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
            QueuePackageUninstall(PackageBuilder.BuildPackageId(feature.Descriptor.Extension.Id, feature.Descriptor.Extension.ExtensionType));
        }

        public void QueuePackageUninstall(string packageId) {
            _processingEngine.AddTask(
                _shellSettings,
                _shellDescriptorManager.GetShellDescriptor(),
                "IPackageUninstallHandler.UninstallPackage",
                new Dictionary<string, object> { { "packageId", packageId } });
        }

        public void UninstallPackage(string packageId) {
            _packageInstaller.Uninstall(packageId, _hostEnvironment.MapPath("~/"));
        }
    }
}