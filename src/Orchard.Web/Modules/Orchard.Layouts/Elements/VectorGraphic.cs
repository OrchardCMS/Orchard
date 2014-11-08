using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class VectorGraphic : Element {
        public override string Category {
            get { return "Media"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public int? MediaId {
            get { return State.Get("MediaId").ToInt32(); }
            set { State["MediaId"] = value.ToString(); }
        }

        public int? Width {
            get { return State.Get("Width").ToInt32(); }
            set { State["Width"] = value.ToString(); }
        }

        public int? Height {
            get { return State.Get("Height").ToInt32(); }
            set { State["Height"] = value.ToString(); }
        }
    }
}