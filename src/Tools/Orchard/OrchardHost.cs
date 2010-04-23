using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Orchard.Host;
using Orchard.Parameters;

namespace Orchard {
    class OrchardHost {
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly string[] _args;
        private OrchardParameters _arguments;

        public OrchardHost(TextReader input, TextWriter output, string[] args) {
            _input = input;
            _output = output;
            _args = args;
        }

        private bool Verbose {
            get { return _arguments != null && _arguments.Verbose; }
        }

        public int Run() {
            try {
                return DoRun();
            }
            catch (Exception e) {
                _output.WriteLine("Error:");
                for (; e != null; e = e.InnerException) {
                    _output.WriteLine("  {0}", e.Message);
                    if (Verbose) {
                        _output.WriteLine("   Stacktrace:");
                        _output.WriteLine("{0}", e.StackTrace);
                        _output.WriteLine();
                    }
                }
                return -1;
            }
        }

        private int DoRun() {
            _arguments = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(_args));
            if (!_arguments.Arguments.Any() || _arguments.Switches.ContainsKey("?")) {
                return GeneralHelp();
            }

            if (string.IsNullOrEmpty(_arguments.VirtualPath))
                _arguments.VirtualPath = "/";
            LogInfo("Virtual path: \"{0}\"", _arguments.VirtualPath);

            if (string.IsNullOrEmpty(_arguments.WorkingDirectory))
                _arguments.WorkingDirectory = Environment.CurrentDirectory;
            LogInfo("Working directory: \"{0}\"", _arguments.WorkingDirectory);

            LogInfo("Detecting orchard installation root directory...");
            var orchardDirectory = GetOrchardDirectory(_arguments.WorkingDirectory);
            LogInfo("Orchard root directory: \"{0}\"", orchardDirectory.FullName);

            LogInfo("Creating ASP.NET AppDomain for command agent...");
            var host = (CommandHost)CreateWorkerAppDomainWithHost(_arguments.VirtualPath, orchardDirectory.FullName, typeof(CommandHost));

            LogInfo("Executing command in ASP.NET AppDomain...");
            var result = host.RunCommand(_input, _output, _arguments);
            LogInfo("Return code for command: {0}", result);

            return result;
        }

        private int GeneralHelp() {
            _output.WriteLine("Executes Orchard commands from a Orchard installation directory.");
            _output.WriteLine("");
            _output.WriteLine("Usage:");
            _output.WriteLine("   orchard.exe arg1 ... argn /switch1[:value1] ... /switchn[:valuen]");
            _output.WriteLine("");
            _output.WriteLine("   arg1 ... argn");
            _output.WriteLine("       Specify which command to execute and optional arguments");
            _output.WriteLine("");
            _output.WriteLine("   /switch1[:value1] ... /switchn[:valuen]");
            _output.WriteLine("       Specify switches to apply to the command. Available switches generally ");
            _output.WriteLine("       depend on the command executed, with the exception of a few built-in ones.");
            _output.WriteLine("");
            _output.WriteLine("   Built-in commands");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   help commands");
            _output.WriteLine("       Display the list of available commands.");
            _output.WriteLine("");
            _output.WriteLine("   help <command-name>");
            _output.WriteLine("       Display help for a given command.");
            _output.WriteLine("");
            _output.WriteLine("   Built-in switches");
            _output.WriteLine("   =================");
            _output.WriteLine("");
            _output.WriteLine("   /WorkingDirectory");
            _output.WriteLine("   /wd");
            _output.WriteLine("       Specifies the orchard installation directory. The current directory is the default.");
            _output.WriteLine("");
            _output.WriteLine("   /Verbose");
            _output.WriteLine("   /v");
            _output.WriteLine("       Turn on verbose output");
            _output.WriteLine("");
            _output.WriteLine("   /VirtualPath");
            _output.WriteLine("   /vp");
            _output.WriteLine("       Virtual path to pass to the WebHost. Empty (i.e. root path) by default.");
            _output.WriteLine("");
            _output.WriteLine("   /Tenant:tenant-name");
            _output.WriteLine("   /t:tenant-name");
            _output.WriteLine("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            _output.WriteLine("");
            return 0;
        }

        private void LogInfo(string format, params object[] args) {
            if (Verbose) {
                _output.Write("{0}: ", DateTime.Now);
                _output.WriteLine(format, args);
            }
        }

        private DirectoryInfo GetOrchardDirectory(string directory) {
            for (var directoryInfo = new DirectoryInfo(directory); directoryInfo != null; directoryInfo = directoryInfo.Parent) {
                if (!directoryInfo.Exists) {
                    throw new ApplicationException(string.Format("Directory \"{0}\" does not exist", directoryInfo.FullName));
                }

                // We look for 
                // 1) .\web.config
                // 2) .\bin\Orchard.Framework.dll
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

            throw new ApplicationException(
                string.Format("Directory \"{0}\" doesn't seem to contain an Orchard installation", new DirectoryInfo(directory).FullName));
        }

        private static object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType) {
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
                new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

            // create Host in the worker app domain
            return appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
        }
    }
}
