using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yaml.Grammar;

namespace Orchard.Packages {
    public class PackageDescriptor {
        public string Location { get; set; }
        public string Name { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
    }
}
