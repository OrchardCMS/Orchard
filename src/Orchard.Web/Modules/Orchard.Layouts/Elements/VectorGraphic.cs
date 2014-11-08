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
    }
}