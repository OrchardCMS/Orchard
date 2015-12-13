using System.Collections.Generic;

namespace Orchard.Taxonomies.Models {
    public class TermPartNode {
        public TermPartNode() {
            Items = new List<TermPartNode>();
        }

        public List<TermPartNode> Items { get; set; }
        public TermPartNode Parent { get; set; }
        public TermPart TermPart { get; set; }
        public int Level { get; set; }
        public dynamic Shape { get; set; }
    }
}