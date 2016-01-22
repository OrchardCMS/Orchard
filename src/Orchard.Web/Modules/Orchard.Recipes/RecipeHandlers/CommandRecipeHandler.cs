using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orchard.Commands;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeHandlers  {
    public class CommandRecipeHandler : IRecipeHandler {
        private readonly ICommandManager _commandManager;
        private readonly CommandParser _commandParser;
        private readonly IRecipeJournal _recipeJournal;

        public CommandRecipeHandler(ICommandManager commandManager, IRecipeJournal recipeJournal) {
            _commandManager = commandManager;
            _recipeJournal = recipeJournal;
            _commandParser = new CommandParser();
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        /* 
         <Command>
            command1
            command2
            command3
         </Command>
        */
        // run Orchard commands.
        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            if (!String.Equals(recipeContext.RecipeStep.Name, "Command", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var commands = 
                recipeContext.RecipeStep.Step.Value
                .Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(commandEntry => commandEntry.Trim());

            foreach (var command in commands) {
                if (!String.IsNullOrEmpty(command)) {
                    if (!String.IsNullOrEmpty(recipeContext.ExecutionId)) {
                        _recipeJournal.WriteJournalEntry(recipeContext.ExecutionId, T("Commands: Executing item {0}.", command).Text);
                    }
                    var commandParameters = _commandParser.ParseCommandParameters(command);
                    var input = new StringReader("");
                    var output = new StringWriter();
                    _commandManager.Execute(new CommandParameters { Arguments = commandParameters.Arguments, Input = input, Output = output, Switches = commandParameters.Switches });
                }
            }

            recipeContext.Executed = true;
        }
    }

    // Utility class for parsing lines of commands.
    // Note: This lexer handles double quotes pretty harshly by design. 
    // In case you needed them in your arguments, hopefully single quotes work for you as a replacement on the receiving end. 
    class CommandParser {
        public CommandParameters ParseCommandParameters(string command) {
            var args = SplitArgs(command);
            var arguments = new List<string>();
            var result = new CommandParameters {
                Switches = new Dictionary<string, string>()
            };

            foreach (var arg in args) {
                if (arg.StartsWith("/")) {
                    //If arg is not empty and starts with '/'

                    int index = arg.IndexOf(':');
                    var switchName = (index < 0 ? arg.Substring(1) : arg.Substring(1, index - 1));
                    var switchValue = (index < 0 || index >= arg.Length ? string.Empty : arg.Substring(index + 1));

                    if (string.IsNullOrEmpty(switchName))
                    {
                        throw new ArgumentException(string.Format("Invalid switch syntax: \"{0}\". Valid syntax is /<switchName>[:<switchValue>].", arg));
                    }

                    result.Switches.Add(switchName, switchValue);
                }
                else {
                    arguments.Add(arg);
                }
            }

            result.Arguments = arguments;
            return result;
        }

        class State {
            private readonly string _commandLine;
            private readonly StringBuilder _stringBuilder;
            private readonly List<string> _arguments;
            private int _index;

            public State(string commandLine) {
                _commandLine = commandLine;
                _stringBuilder = new StringBuilder();
                _arguments = new List<string>();
            }

            public StringBuilder StringBuilder { get { return _stringBuilder; } }
            public bool EOF { get { return _index >= _commandLine.Length; } }
            public char Current { get { return _commandLine[_index]; } }
            public IEnumerable<string> Arguments { get { return _arguments; } }

            public void AddArgument() {
                _arguments.Add(StringBuilder.ToString());
                StringBuilder.Clear();
            }

            public void AppendCurrent() {
                StringBuilder.Append(Current);
            }

            public void Append(char ch) {
                StringBuilder.Append(ch);
            }

            public void MoveNext() {
                if (!EOF)
                    _index++;
            }
        }

        /// <summary>
        /// Implement the same logic as found at
        /// http://msdn.microsoft.com/en-us/library/17w5ykft.aspx
        /// The 3 special characters are quote, backslash and whitespaces, in order 
        /// of priority.
        /// The semantics of a quote is: whatever the state of the lexer, copy
        /// all characters verbatim until the next quote or EOF.
        /// The semantics of backslash is: If the next character is a backslash or a quote,
        /// copy the next character. Otherwise, copy the backslash and the next character.
        /// The semantics of whitespace is: end the current argument and move on to the next one.
        /// </summary>
        private static IEnumerable<string> SplitArgs(string commandLine) {
            var state = new State(commandLine);
            while (!state.EOF) {
                switch (state.Current) {
                    case '"':
                        ProcessQuote(state);
                        break;

                    case '\\':
                        ProcessBackslash(state);
                        break;

                    case ' ':
                    case '\t':
                        if (state.StringBuilder.Length > 0)
                            state.AddArgument();
                        state.MoveNext();
                        break;

                    default:
                        state.AppendCurrent();
                        state.MoveNext();
                        break;
                }
            }
            if (state.StringBuilder.Length > 0)
                state.AddArgument();
            return state.Arguments;
        }

        private static void ProcessQuote(State state) {
            state.MoveNext();
            while (!state.EOF) {
                if (state.Current == '"') {
                    state.MoveNext();
                    break;
                }
                state.AppendCurrent();
                state.MoveNext();
            }

            state.AddArgument();
        }

        private static void ProcessBackslash(State state) {
            state.MoveNext();
            if (state.EOF) {
                state.Append('\\');
                return;
            }

            if (state.Current == '"') {
                state.Append('"');
                state.MoveNext();
            }
            else {
                state.Append('\\');
                state.AppendCurrent();
                state.MoveNext();
            }
        }
    }
}