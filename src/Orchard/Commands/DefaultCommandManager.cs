using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Features.Metadata;

namespace Orchard.Commands {
    public class DefaultCommandManager : ICommandManager {
        private readonly IEnumerable<Meta<Func<ICommandHandler>>> _handlers;

        public DefaultCommandManager(IEnumerable<Meta<Func<ICommandHandler>>> handlers) {
            _handlers = handlers;
        }

        public void Execute(CommandParameters parameters) {
            var matches = MatchCommands(parameters);

            // Workaround autofac integration: module registration is currently run twice...
            //if (matches.Count() == 1) {
            //  var match = matches.Single();
            if (matches.Count() == 2) {
                var match = matches.First();
                match.CommandHandlerFactory().Execute(match.Context);
            }
            else if (matches.Any()) {
                // too many
            }
            else {
                // none
            }
        }

        public IEnumerable<CommandDescriptor> GetCommandDescriptors() {
            return _handlers
                .SelectMany(h => GetDescriptor(h.Metadata).Commands)
                // Workaround autofac integration: module registration is currently run twice...
                .Distinct(new CommandsComparer());
        }

        private IEnumerable<Match> MatchCommands(CommandParameters parameters) {
            foreach (var argCount in Enumerable.Range(1, parameters.Arguments.Count()).Reverse()) {
                int count = argCount;
                var matches = _handlers.SelectMany(h => MatchCommands(parameters, count, GetDescriptor(h.Metadata), h.Value));
                if (matches.Any())
                    return matches;
            }

            return Enumerable.Empty<Match>();
        }

        private static IEnumerable<Match> MatchCommands(CommandParameters parameters, int argCount, CommandHandlerDescriptor descriptor, Func<ICommandHandler> handlerFactory) {
            foreach (var commandDescriptor in descriptor.Commands) {
                var names = commandDescriptor.Name.Split(' ');
                if (!parameters.Arguments.Take(argCount).SequenceEqual(names, StringComparer.OrdinalIgnoreCase)) {
                    // leading arguments not equal to command name
                    continue;
                }

                yield return new Match {
                    Context = new CommandContext {
                        Arguments = parameters.Arguments.Skip(names.Count()),
                        Command = string.Join(" ", names),
                        CommandDescriptor = commandDescriptor,
                        Input = parameters.Input,
                        Output = parameters.Output,
                        Switches = parameters.Switches,
                    },
                    CommandHandlerFactory = handlerFactory
                };
            }
        }

        private static CommandHandlerDescriptor GetDescriptor(IDictionary<string, object> metadata) {
            return ((CommandHandlerDescriptor)metadata[typeof(CommandHandlerDescriptor).FullName]);
        }

        private class Match {
            public CommandContext Context { get; set; }
            public Func<ICommandHandler> CommandHandlerFactory { get; set; }
        }

        public class CommandsComparer : IEqualityComparer<CommandDescriptor> {
            public bool Equals(CommandDescriptor x, CommandDescriptor y) {
                return x.MethodInfo.Equals(y.MethodInfo);
            }

            public int GetHashCode(CommandDescriptor obj) {
                return obj.MethodInfo.GetHashCode();
            }
        }
    }
}
