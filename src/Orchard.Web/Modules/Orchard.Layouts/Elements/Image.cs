using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class Image : Element {
        public override string Category {
            get { return "Media"; }
        }

        public override bool HasEditor {
            get { return true; }
        }

        public int? ImageId {
            get { return State.Get("ImageId").ToInt32(); }
            set { State["ImageId"] = value.ToString(); }
        }
    }
}