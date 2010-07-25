using System;
using System.IO;
using Orchard;
using Orchard.HostContext;
using Orchard.Parameters;
using Orchard.ResponseFiles;

namespace OrchardCLI {
    class CLIHost {
        private readonly TextWriter _output;
        private readonly TextReader _input;
        private readonly ICommandHostContextProvider _commandHostContextProvider;

        public CLIHost(TextReader input, TextWriter output, string[] args) {
            _input = input;
            _output = output;
            _commandHostContextProvider = new CommandHostContextProvider(args);
        }

        public int Run() {
            try {
                return DoRun();
            }
            catch (Exception e) {
                _output.WriteLine("Error:");
                for (; e != null; e = e.InnerException) {
                    _output.WriteLine("  {0}", e.Message);
                }
                return -1;
            }
        }


        public int DoRun() {
            var context = CommandHostContext();
            if (context.ShowHelp) {
                DisplayHelp();
                return 0;
            }

            _output.WriteLine("Type \"help commands\" for help, \"exit\" to exit, \"cls\" to clear screen");
            while (true) {
                var command = ReadCommand(context);
                switch (command.ToLowerInvariant()) {
                    case "quit":
                    case "q":
                    case "exit":
                    case "e":
                        _commandHostContextProvider.Shutdown(context);
                        return 0;
                    case "cls":
                        Console.Clear();
                        break;
                    default:
                        context = RunCommand(context, command);
                        break;
                }
            }
        }

        private string ReadCommand(CommandHostContext context) {
            _output.WriteLine();
            _output.Write("orchard> ");
            return _input.ReadLine();
        }

        private CommandHostContext CommandHostContext() {
            _output.WriteLine("Initializing Orchard session. (This might take a few seconds...)");
            var result = _commandHostContextProvider.CreateContext(true/*interactive*/);
            if (result.StartSessionResult == result.RetryResult) {
                result = _commandHostContextProvider.CreateContext(true/*interactive*/);
            }
            return result;
        }

        private CommandHostContext RunCommand(CommandHostContext context, string command) {
            if (string.IsNullOrWhiteSpace(command))
                return context;

            int result = RunCommandInSession(context, command);
            if (result == context.RetryResult) {
                _commandHostContextProvider.Shutdown(context);
                context = CommandHostContext();
                result = RunCommandInSession(context, command);
                if (result != 0)
                    _output.WriteLine("Command returned non-zero result: {0}", result);
            }
            return context;
        }

        private int RunCommandInSession(CommandHostContext context, string command) {
            try {
                var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(ResponseFileReader.SplitArgs(command)));
                return context.CommandHost.RunCommandInSession(_input, _output, context.Logger, args);
            }
            catch (AppDomainUnloadedException) {
                _output.WriteLine("AppDomain of Orchard session has been unloaded. (Retrying...)");
                return context.RetryResult;
            }
        }

        private void DisplayHelp() {
            _output.WriteLine("Executes the Orchard interactive from a Orchard installation directory.");
            _output.WriteLine("");
            _output.WriteLine("Usage:");
            _output.WriteLine("   orchardcli.exe [/switch1[:value1]] ... [/switchn[:valuen]]");
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
            

        }
    }
}
