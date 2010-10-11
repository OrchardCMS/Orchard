using System.Collections.Generic;
using System.Web;

namespace Orchard.DisplayManagement.Shapes {
    public class ShapeMetadata {
        public ShapeMetadata() {
            Wrappers = new List<string>();
            Alternates = new List<string>();
        }

        public string Type { get; set; }
        public string Position { get; set; }
        public string Prefix { get; set; }
        public IList<string> Wrappers { get; set; }
        public IList<string> Alternates { get; set; }

        public bool WasExecuted { get; set; }
        public IHtmlString ChildContent { get; set; }
    }
}