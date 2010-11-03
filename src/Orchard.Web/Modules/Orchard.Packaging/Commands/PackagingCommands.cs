using System;
using System.IO;
using System.Web.Hosting;
using Orchard.Commands;
using Orchard.Environment.Extensions;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Commands {
    [OrchardFeature("Orchard.Packaging")]
    public class PackagingCommands : DefaultOrchardCommandHandler {
        private static readonly string OrchardWebProj = HostingEnvironment.MapPath("~/Orchard.Web.csproj");
        private const string PackagePath = "Packages";

        private readonly IPackageManager _packageManager;

        public PackagingCommands(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

        [CommandHelp("package create <moduleName>\r\n\t" + "Create a package for the module <moduleName>. The default filename is Orchard.<extension>.<moduleName>.<moduleVersion>.nupkg.")]
        [CommandName("package create")]
        public void CreatePackage(string moduleName) {
            var packageData = _packageManager.Harvest(moduleName);
            if (packageData == null) {
                Context.Output.WriteLine(T("Module \"{0}\" does not exist in this Orchard installation.", moduleName));
                return;
            }

            // append "Orchard.[ExtensionType]" to prevent conflicts with other packages (e.g, TinyMce, jQuery, ...)
            var filename = string.Format("Orchard.{0}.{1}.{2}.nupkg", packageData.ExtensionType, packageData.ExtensionName, packageData.ExtensionVersion);
            var packagePath = Path.Combine(Directory.GetParent(OrchardWebProj).FullName, PackagePath);

            if(!Directory.Exists(packagePath)) {
                Directory.CreateDirectory(packagePath);
            }

            // packages are created in a specific folder otherwise they are in /bin, which crashed the current shell
            filename = Path.Combine(packagePath, filename);

            using ( var stream = File.Create(filename) ) {
                packageData.PackageStream.CopyTo(stream);
                stream.Close();
            }

            var fileInfo = new FileInfo(filename);
            Context.Output.WriteLine(T("Package \"{0}\" successfully created", fileInfo.FullName));
        }

        [CommandHelp("package install <filename>\r\n\t" + "Install a module from a package <filename>.")]
        [CommandName("package install")]
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
