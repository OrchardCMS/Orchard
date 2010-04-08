using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard {
    public class OrchardArguments {
        public bool Verbose { get; set; }
        public string VirtualPath { get; set; }
        public string WorkingDirectory { get; set; }
        public string Tenant { get; set; }
    }
}