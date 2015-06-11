using System.Collections.Generic;

namespace Orchard.Commands {
    public interface ICommandManager : IDependency {
        void Execute(CommandParameters parameters);
        IEnumerable<CommandDescriptor> GetCommandDescriptors();
    }
}