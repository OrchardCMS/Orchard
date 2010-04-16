using System;
using System.Collections.Generic;

namespace Orchard.Extensions.Models {
    public class Feature {
        public string ExtensionName { get; set; }
        public string Name { get; set; }
        public IEnumerable<Type> Types { get; set; }
    }
}
