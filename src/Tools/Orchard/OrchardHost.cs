using System;
using System.Linq;
using System.IO;
using Orchard.HostContext;

namespace Orchard {
    class OrchardHost {
        private readonly TextReader _input;
        private readonly TextWriter _output;
        private readonly ICommandHostContextProvider _commandHostContextProvider;

        public OrchardHost(TextReader input, TextWriter output, string[] args) {
            _input = input;
            _output = output;
            _commandHostContextProvider = new CommandHostContextProvider(args);
        }

        public int Run() {
            try {
                return DoRun();
            }
            catch (Exception e) {
                _output.WriteLine("Error running command:");
                for (; e != null; e = e.InnerException) {
                    _output.WriteLine("  {0}", e.Message);
                }
                return -1;
            }
        }

        private int DoRun() {
            var context = CommandHostContext();
            if (context.ShowHelp) {
                DisplayHelp();
                return 0;
            }

            var result = Execute(context);
            _commandHostContextProvider.Shutdown(context);
            return result;
        }

        private CommandHostContext CommandHostContext() {
            _output.WriteLine("Initializing Orchard host. (This might take a few seconds...)");
            var result = _commandHostContextProvider.CreateContext(false/*interactive*/);
            if (result.StartSessionResult == result.RetryResult) {
                result = _commandHostContextProvider.CreateContext(false/*interactive*/);
            }
            return result;
        }

        private int Execute(CommandHostContext context) {
            if (context.Arguments.ResponseFiles.Any()) {
                var responseLines = new ResponseFiles.ResponseFiles().ReadFiles(context.Arguments.ResponseFiles);
                return context.CommandHost.RunCommands(_input, _output, context.Logger, responseLines.ToArray());
            }
            else {
                return context.CommandHost.RunCommand(_input, _output, context.Logger, context.Arguments);
            }
        }

        private void DisplayHelp() {
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
            return;
        }
    }
}
