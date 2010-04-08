using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Features.Metadata;

namespace Orchard.Commands {
    public interface ICommandManager : IDependency{
        void Execute(CommandContext context);
    }

    public class CommandManager : ICommandManager {
        private readonly IEnumerable<Meta<ICommandHandler>> _handlers;

        public CommandManager(IEnumerable<Meta<ICommandHandler>> handlers) {
            _handlers = handlers;
        }

        public void Execute(CommandContext context) {
        }
    }
}
