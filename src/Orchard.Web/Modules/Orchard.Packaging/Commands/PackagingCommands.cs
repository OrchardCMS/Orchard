using System.IO;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Commands {
    [OrchardFeature("Orchard.Packaging")]
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private readonly IPackageManager _packageManager;
        private readonly INotifier _notifier;

        public PackagingCommands(IPackageManager packageManager, INotifier notifier) {
            _packageManager = packageManager;
            _notifier = notifier;
        }

        [OrchardSwitch]
        public string Filename { get; set; }

        [CommandHelp("packaging create package <moduleName>\r\n\t" + "Create a package for the module <moduleName>. The default filename is <moduleName>-<moduleVersion>.zip.")]
        [CommandName("packaging create package")]
        [OrchardSwitches("Filename")]
        public void CreatePackage(string moduleName) {
            var packageData = _packageManager.Harvest(moduleName);
            if (packageData == null) {
                Context.Output.WriteLine(T("Module \"{0}\" does not exist in this Orchard installation.", moduleName));
                return;
            }

            var filename = string.Format("{0}-{1}.zip", packageData.ExtensionName, packageData.ExtensionVersion);

            using(var stream = File.Create(filename)) {
                packageData.PackageStream.CopyTo(stream);
                stream.Close();
            }

            var fileInfo = new FileInfo(filename);
            foreach (var entry in _notifier.List()) {
                Context.Output.WriteLine(entry.Message);
            }
            Context.Output.WriteLine(T("Package \"{0}\" successfully created", fileInfo.FullName));
        }

        [CommandHelp("packaging install package <filename>\r\n\t" + "Install a module from a package <filename>.")]
        [CommandName("packaging install package")]
        public void InstallPackage(string filename) {
            if (!File.Exists(filename)) {
                Context.Output.WriteLine(T("File \"{0}\" does not exist.", filename));
            }

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var packageInfo = _packageManager.Install(stream);
                foreach (var entry in _notifier.List()) {
                    Context.Output.WriteLine(entry.Message);
                }
            }
        }
    }
}
