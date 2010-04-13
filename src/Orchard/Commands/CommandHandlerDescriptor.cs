using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Commands {
    public class CommandHandlerDescriptor {
        public IEnumerable<CommandDescriptor> Commands { get; set; }
    }
}
