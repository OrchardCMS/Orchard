using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Descriptors {
    public class PlacementInfo {
        public PlacementInfo() {
            Alternates = Enumerable.Empty<string>();
            Wrappers = Enumerable.Empty<string>();
        }

        public string Location { get; set; }
        public string Source { get; set; }

        public string ShapeType { get; set; }
        public IEnumerable<string> Alternates { get; set; }
        public IEnumerable<string> Wrappers { get; set; }
    }
}
