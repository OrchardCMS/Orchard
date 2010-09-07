using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.DisplayManagement.Shapes {
    public class ShapeMetadata {
        public ShapeMetadata() {
            FrameTypes = new List<string>();
        }

        public string Type { get; set; }
        public string Position { get; set; }
        public IList<string> FrameTypes { get; set; }

        public bool WasExecuted { get; set; }
        public IHtmlString ChildContent { get; set; }
    }
}