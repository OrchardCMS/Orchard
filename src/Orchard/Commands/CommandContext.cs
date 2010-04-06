using System.Collections.Generic;
using System.Collections.Specialized;

namespace Orchard.Commands {
    public class CommandContext {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Command { get; set; }
        public IEnumerable<string> Arguments { get; set; }
        public NameValueCollection Switches { get; set; }
    }
}
