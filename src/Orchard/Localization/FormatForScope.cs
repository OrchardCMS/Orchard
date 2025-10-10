using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Localization {
    public class FormatForScope {
        public FormatForScope(string format, string scope) {
            Scope = scope;
            Format = format;
        }
        public string Scope { get; set; }
        public string Format { get; set; }
    }
}
