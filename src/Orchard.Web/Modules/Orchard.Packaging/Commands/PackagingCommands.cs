using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Commands {
    [OrchardFeature("Orchard.Packaging")]
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private static readonly string ApplicationPath = HostingEnvironment.MapPath("~/");

        private readonly IPackageManager _packageManager;
        private readonly INotifier _notifier;

        public PackagingCommands(IPackageManager packageManager, INotifier notifier) {
            _packageManager = packageManager;
            _notifier = notifier;
        }

        [OrchardSwitch]
        public string Version { get; set; }

        [CommandHelp("package create <extensionName> <path> \r\n\t" + "Create a package for the module <moduleName>. The default filename is Orchard.<extension>.<extensionName>.<moduleVersion>.nupkg.")]
        [CommandName("package create")]
        public void CreatePackage(string extensionName, string path) {
            var packageData = _packageManager.Harvest(extensionName);
            if (packageData == null) {
                Context.Output.WriteLine(T("Module or Theme \"{0}\" does not exist in this Orchard installation.", extensionName));
                return;
            }

            // append "Orchard.[ExtensionType]" to prevent conflicts with other packages (e.g, TinyMce, jQuery, ...)
            var filename = string.Format("Orchard.{0}.{1}.{2}.nupkg", packageData.ExtensionType, packageData.ExtensionName, packageData.ExtensionVersion);

            if ( !Directory.Exists(path) ) {
                Directory.CreateDirectory(path);
            }

            // packages are created in a specific folder otherwise they are in /bin, which crashed the current shell
            filename = Path.Combine(path, filename);

            using ( var stream = File.Create(filename) ) {
                packageData.PackageStream.CopyTo(stream);
                stream.Close();
            }

            var fileInfo = new FileInfo(filename);
            Context.Output.WriteLine(T("Package \"{0}\" successfully created", fileInfo.FullName));
        }

        [CommandHelp("package install <packageId> <location> /Version:<version> \r\n\t" + "Install a module or a theme from a package file.")]
        [CommandName("package install")]
        [OrchardSwitches("Version")]
        public void InstallPackage(string packageId, string location) {
            try {
                _packageManager.Install(packageId, Version, Path.GetFullPath(location), ApplicationPath);

                foreach (var message in _notifier.List()) {
                    Context.Output.WriteLine(message.Message);
                }
            }
            catch(Exception e) {
                // Exceptions area thrown by NuGet as error messages
                Context.Output.WriteLine(HttpUtility.HtmlDecode(T("Could not install the package: {0}", e.Message).Text));
            }
        }

        [CommandHelp("package uninstall <packageId> \r\n\t" + "Uninstall a module or a theme.")]
        [CommandName("package uninstall")]
        public void UninstallPackage(string packageId) {
            try {
                _packageManager.Uninstall(packageId, ApplicationPath);

                foreach ( var message in _notifier.List() ) {
                    Context.Output.WriteLine(message.Message);
                }
            }
            catch(Exception e) {
                // Exceptions area thrown by NuGet as error messages
                Context.Output.WriteLine(HttpUtility.HtmlDecode(T("Could not unintall the package: {0}", e.Message).Text));
            }

        }
    }
}
