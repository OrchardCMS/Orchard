using System.IO;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.Commands {
    [OrchardFeature("Packaging")]
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private readonly IPackageManager _packageManager;

        public PackagingCommands(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

        [OrchardSwitch]
        public string Filename { get; set; }

        [CommandHelp("module create package <moduleName> [/Filename:filename.zip]\r\n\t" + "Create a package for the module <moduleName>. The default filename is <moduleName>-<moduleVersion>.zip.")]
        [CommandName("module create package")]
        [OrchardSwitches("Filename")]
        public void CreatePackage(string moduleName) {
            var packageData = _packageManager.Harvest(moduleName);
            if (packageData == null) {
                Context.Output.WriteLine(T("Module \"{0}\" does not exist in this Orchard installation.", moduleName));
                return;
            }

            var filename = Filename;
            if(string.IsNullOrEmpty(filename)) {
                filename = string.Format("{0}-{1}.zip", packageData.ExtensionName, packageData.ExtensionVersion);
            }

            using(var stream = File.Create(filename)) {
                packageData.PackageStream.CopyTo(stream);
                stream.Close();
            }

            var fileInfo = new FileInfo(filename);
            Context.Output.WriteLine(T("Package \"{0}\" successfully created", fileInfo.FullName));
        }

        [CommandHelp("module install package <filename>\r\n\t" + "Install a module from a package <filename>.")]
        [CommandName("module install package")]
        public void InstallPackage(string filename) {
            if (!File.Exists(filename)) {
                Context.Output.WriteLine(T("File \"{0}\" does not exist.", filename));
            }

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var packageInfo = _packageManager.Install(stream);
                Context.Output.WriteLine(T("Package \"{0}\" successfully installed at \"{1}\"", packageInfo.ExtensionName, packageInfo.ExtensionPath));
            }
        }
    }
}
