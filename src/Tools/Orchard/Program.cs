using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Orchard.Host;

namespace Orchard {
    class Program {
        private readonly string[] _args;

        private Program(string[] args) {
            _args = args;
        }

        static void Main(string[] args) {
            new Program(args).Run();
        }

        public void Run() {
            // Parse command line arguments
            var arguments = new OrchardArgumentsParser().Parse(new Orchard.Arguments.Parser().Parse(_args));

            if (string.IsNullOrEmpty(arguments.VirtualPath))
                arguments.VirtualPath = "/";

            if (string.IsNullOrEmpty(arguments.WorkingDirectory))
                arguments.WorkingDirectory = Environment.CurrentDirectory;

            if (arguments.Verbose) {
                Console.WriteLine("Virtual path: {0}", arguments.VirtualPath);
                Console.WriteLine("Working directory: {0}", arguments.WorkingDirectory);
            }

            var orchardDirectory = GetOrchardDirectory(arguments.WorkingDirectory);
            if (arguments.Verbose) {
                Console.WriteLine("Detected and using orchard directory \"{0}\"", orchardDirectory.FullName);
            }

            object host = CreateWorkerAppDomainWithHost(arguments.VirtualPath, orchardDirectory.FullName, typeof(CommandHost));

        }

        private DirectoryInfo GetOrchardDirectory(string directory) {
            for (var directoryInfo = new DirectoryInfo(directory); directoryInfo != null; directoryInfo = directoryInfo.Parent) {
                if (!directoryInfo.Exists) {
                    throw new ApplicationException(string.Format("Directory \"{0}\" does not exist", directoryInfo.FullName));
                }

                var webConfigFileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, "web.config"));
                if (!webConfigFileInfo.Exists)
                    continue;

                var binDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "bin"));
                if (!binDirectoryInfo.Exists)
                    continue;

                var orchardFrameworkFileInfo = new FileInfo(Path.Combine(binDirectoryInfo.FullName, "Orchard.Framework.dll"));
                if (!orchardFrameworkFileInfo.Exists)
                    continue;

                return directoryInfo;
            }

            throw new ApplicationException("Directory \"{0}\" doesn't seem to contain an Orchard installation");
        }

        public object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType) {
            // this creates worker app domain in a way that host doesn't need to be in GAC or bin
            // using BuildManagerHost via private reflection
            string uniqueAppString = string.Concat(virtualPath, physicalPath).ToLowerInvariant();
            string appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

            // create BuildManagerHost in the worker app domain
            var appManager = ApplicationManager.GetApplicationManager();
            var buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
            var buildManagerHost = appManager.CreateObject(appId, buildManagerHostType, virtualPath, physicalPath, false);

            // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
            buildManagerHostType.InvokeMember(
                "RegisterAssembly",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null,
                buildManagerHost,
                new object[2] { hostType.Assembly.FullName, hostType.Assembly.Location });

            // create Host in the worker app domain
            return appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
        }

    }
}
