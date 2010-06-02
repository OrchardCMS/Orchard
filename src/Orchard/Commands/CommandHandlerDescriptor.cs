using System.Collections.Generic;

namespace Orchard.Commands {
    public class CommandHandlerDescriptor {
        public IEnumerable<CommandDescriptor> Commands { get; set; }
    }
}
