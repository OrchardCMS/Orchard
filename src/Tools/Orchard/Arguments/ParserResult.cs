using System.Collections.Generic;

namespace Orchard.Arguments {
    public class ParserResult {
        public ParserResult() {
            this.Arguments = new List<string>();
            this.Switches = new List<Switch>();
        }

        public IList<string> Arguments { get; set; }
        public IList<Switch> Switches { get; set; }
    }
}