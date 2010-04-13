using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.Commands {
    public class CommandParameters {
        public IEnumerable<string> Arguments { get; set; }
        public IDictionary<string, string> Switches { get; set; }

        public TextReader Input { get; set; }
        public TextWriter Output { get; set; }
    }
}
