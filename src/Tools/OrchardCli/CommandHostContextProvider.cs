using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Orchard;
using Orchard.Host;
using Orchard.Parameters;

namespace OrchardCLI {
    public class CommandHostContextProvider : ICommandHostContextProvider{
        private readonly string[] _args;
        private TextWriter _output;
        private TextReader _input;

        public CommandHostContextProvider(string[] args) {
            _input = Console.In;
            _output = Console.Out;
            _args = args;
        }

        public CommandHostContext CreateContext() {
            var context = new CommandHostContext();
            Initialize(context);
            return context;
        }

        public void Shutdown(CommandHostContext context) {
            ShutdownHost(context);
        }

        private void Initialize(CommandHostContext context) {
            context.Arguments = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(_args));
            context.Logger = new Logger(context.Arguments.Verbose, _output);

            // Perform some argument validation and display usage if something is incorrect
            bool showHelp = context.Arguments.Switches.ContainsKey("?");
            if (!showHelp) {
                //showHelp = (!_arguments.Arguments.Any() && !_arguments.ResponseFiles.Any());
            }

            if (!showHelp) {
                //showHelp = (_arguments.Arguments.Any() && _arguments.ResponseFiles.Any());
                //if (showHelp) {
                //    _output.WriteLine("Incorrect syntax: Response files cannot be used in conjunction with commands");
                //}
            }

            if (showHelp) {
                GeneralHelp();
                return;
            }

            if (string.IsNullOrEmpty(context.Arguments.VirtualPath))
                context.Arguments.VirtualPath = "/";
            LogInfo(context, "Virtual path: \"{0}\"", context.Arguments.VirtualPath);

            if (string.IsNullOrEmpty(context.Arguments.WorkingDirectory))
                context.Arguments.WorkingDirectory = Environment.CurrentDirectory;
            LogInfo(context, "Working directory: \"{0}\"", context.Arguments.WorkingDirectory);

            LogInfo(context, "Detecting orchard installation root directory...");
            context.OrchardDirectory = GetOrchardDirectory(context.Arguments.WorkingDirectory);
            LogInfo(context, "Orchard root directory: \"{0}\"", context.OrchardDirectory.FullName);

            CreateHost(context);
            context.CommandHost.StartSession(_input, _output);
        }

        private void CreateHost(CommandHostContext context) {
            context.AppManager = ApplicationManager.GetApplicationManager();

            LogInfo(context, "Creating ASP.NET AppDomain for command agent...");
            context.AppObject = CreateWorkerAppDomainWithHost(context.AppManager, context.Arguments.VirtualPath, context.OrchardDirectory.FullName, typeof(CommandHost));
            context.CommandHost = (CommandHost)context.AppObject.ObjectInstance;
        }

        private void ShutdownHost(CommandHostContext context) {
            if (context.CommandHost != null) {
                context.CommandHost.StopSession(_input, _output);
            }

            if (context.AppObject != null) {
                LogInfo(context, "Shutting down ASP.NET AppDomain...");
                context.AppManager.ShutdownApplication(context.AppObject.ApplicationId);
            }
        }

        private int GeneralHelp() {
            _output.WriteLine("Executes Orchard commands from a Orchard installation directory.");
            _output.WriteLine("");
            _output.WriteLine("Usage:");
            _output.WriteLine("   orchard.exe command [arg1] ... [argn] [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("   orchard.exe @response-file1 ... [@response-filen] [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("");
            _output.WriteLine("   command");
            _output.WriteLine("       Specify the command to execute");
            _output.WriteLine("");
            _output.WriteLine("   [arg1] ... [argn]");
            _output.WriteLine("       Specify additional arguments for the command");
            _output.WriteLine("");
            _output.WriteLine("   [/switch1[:value1]] ... [/switchn[:valuen]]");
            _output.WriteLine("       Specify switches to apply to the command. Available switches generally ");
            _output.WriteLine("       depend on the command executed, with the exception of a few built-in ones.");
            _output.WriteLine("");
            _output.WriteLine("   [@response-file1] ... [@response-filen]");
            _output.WriteLine("       Specify one or more response files to be used for reading commands and switches.");
            _output.WriteLine("       A response file is a text file that contains one line per command to execute.");
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
            _output.WriteLine("   /WorkingDirectory:<physical-path>");
            _output.WriteLine("   /wd:<physical-path>");
            _output.WriteLine("       Specifies the orchard installation directory. The current directory is the default.");
            _output.WriteLine("");
            _output.WriteLine("   /Verbose");
            _output.WriteLine("   /v");
            _output.WriteLine("       Turn on verbose output");
            _output.WriteLine("");
            _output.WriteLine("   /VirtualPath:<virtual-path>");
            _output.WriteLine("   /vp:<virtual-path>");
            _output.WriteLine("       Virtual path to pass to the WebHost. Empty (i.e. root path) by default.");
            _output.WriteLine("");
            _output.WriteLine("   /Tenant:tenant-name");
            _output.WriteLine("   /t:tenant-name");
            _output.WriteLine("       Specifies which tenant to run the command into. \"Default\" tenant by default.");
            _output.WriteLine("");
            return 1;
        }

        private void LogInfo(CommandHostContext context, string format, params object[] args) {
            if (context.Logger != null)
                context.Logger.LogInfo(format, args);
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

        private static ApplicationObject CreateWorkerAppDomainWithHost(ApplicationManager appManager, string virtualPath, string physicalPath, Type hostType) {
            // this creates worker app domain in a way that host doesn't need to be in GAC or bin
            // using BuildManagerHost via private reflection
            string uniqueAppString = string.Concat(virtualPath, physicalPath).ToLowerInvariant();
            string appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

            // create BuildManagerHost in the worker app domain
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
            return new ApplicationObject {
                ApplicationId = appId,
                ObjectInstance = appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false)
            };
        }
    }
}
