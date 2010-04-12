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
            var matches = _handlers.SelectMany(h => MatchCommands(parameters, GetDescriptor(h.Metadata), h.Value));

            if (matches.Count() == 1) {
                var match = matches.Single();
                match.CommandHandlerFactory().Execute(match.Context);
                parameters.Output = match.Context.Output;
            }
            else if (matches.Any()) {
                // too many
            }
            else {
                // none
            }
        }

        public IEnumerable<CommandDescriptor> GetCommandDescriptors() {
            return _handlers.SelectMany(h => GetDescriptor(h.Metadata).Commands);
        }

        private class Match {
            public CommandContext Context { get; set; }
            public Func<ICommandHandler> CommandHandlerFactory { get; set; }
        }

        private static IEnumerable<Match> MatchCommands(CommandParameters parameters, CommandHandlerDescriptor descriptor, Func<ICommandHandler> handlerFactory) {
            foreach (var commandDescriptor in descriptor.Commands) {
                string[] names = commandDescriptor.Name.Split(' ');
                if (!parameters.Arguments.Take(names.Count()).SequenceEqual(names, StringComparer.OrdinalIgnoreCase)) {
                    // leading arguments not equal to command name
                    continue;
                }

                yield return new Match {
                    Context = new CommandContext {
                        Arguments = parameters.Arguments.Skip(names.Count()),
                        Command = string.Join(" ",names),
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
    }
}
