using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Orchard.Commands {
    public class CommandContext {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Command { get; set; }
        public IEnumerable<string> Arguments { get; set; }
        public IDictionary<string,string> Switches { get; set; }
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
