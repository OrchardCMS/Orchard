using System;
using System.IO;
using Orchard;
using Orchard.Parameters;
using Orchard.ResponseFiles;

namespace OrchardCLI {
    class CLIHost {
        private readonly TextWriter _output;
        private readonly TextReader _input;
        private readonly ICommandHostContextProvider _commandHostContextProvider;

        public CLIHost(string[] args) {
            _input = Console.In;
            _output = Console.Out;
            _commandHostContextProvider = new CommandHostContextProvider(args);
        }

        public int Run() {
            var context = CommandHostContext();
            Console.WriteLine("Type \"help commands\" for help, \"exit\" to exit");
            while (true) {
                var command = ReadCommand(context);
                switch (command.ToLowerInvariant()) {
                    case "quit":
                    case "q":
                    case "exit":
                    case "e":
                        _commandHostContextProvider.Shutdown(context);
                        return 0;
                    default:
                        context = RunCommand(context, command);
                        break;
                }
            }
        }

        private string ReadCommand(CommandHostContext context) {
            Console.WriteLine();
            Console.Write("orchard> ");
            return Console.ReadLine();
        }

        private CommandHostContext CommandHostContext() {
            Console.WriteLine("Initializing Orchard session... (This might take a few seconds)");
            var result = _commandHostContextProvider.CreateContext();
            if (result.StartSessionResult == 240/*special return code for "Retry"*/) {
                result = _commandHostContextProvider.CreateContext();
            }
            return result;
        }

        private CommandHostContext RunCommand(CommandHostContext context, string command) {
            if (string.IsNullOrWhiteSpace(command))
                return context;

            int result = RunCommandInSession(context, command);
            if(result == 240) {
                if (result == 240/*special return code for "Retry"*/) {
                    _commandHostContextProvider.Shutdown(context);
                    context = CommandHostContext();
                    result = RunCommandInSession(context, command);
                    if (result != 0)
                        Console.WriteLine("Command returned non-zero result: {0}", result);
                }
            }
            return context;
        }

        private int RunCommandInSession(CommandHostContext context, string command) {
            var args = new OrchardParametersParser().Parse(new CommandParametersParser().Parse(ResponseFileReader.SplitArgs(command)));
            return context.CommandHost.RunCommandInSession(_input, _output, context.Logger, args);
        }
    }
}
