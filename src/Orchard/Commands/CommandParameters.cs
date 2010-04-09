using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Commands {
    public class CommandParameters {
        public IEnumerable<string> Arguments { get; set; }
        public IDictionary<string, string> Switches { get; set; }

        public string Input { get; set; }
        public string Output { get; set; }
    }
}
