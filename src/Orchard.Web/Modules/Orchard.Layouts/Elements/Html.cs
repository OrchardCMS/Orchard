using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Html : Element {
        public override string Category {
            get { return "Content"; }
        }

        public override bool HasEditor {
            get { return true; }
        }
        
        public string Content {
            get { return State.Get("Content"); }
            set { State["Content"] = value; }
        }
    }
}