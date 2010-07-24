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

        public CLIHost(TextReader input, TextWriter output, string[] args) {
            _input = input;
            _output = output;
            _commandHostContextProvider = new CommandHostContextProvider(args);
        }

        public int Run() {
            var context = CommandHostContext();
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
            var result = _commandHostContextProvider.CreateContext();
            if (result.StartSessionResult == result.RetryResult) {
                result = _commandHostContextProvider.CreateContext();
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
    }
}
