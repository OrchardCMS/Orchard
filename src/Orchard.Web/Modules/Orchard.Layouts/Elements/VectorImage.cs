using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements {
    public class VectorImage : Element {
        public override string Category {
            get { return "Media"; }
        }

        public override string ToolboxIcon {
            get { return "\uf1c5"; }
        }

        public int? MediaId {
            get { return this.Retrieve(x => x.MediaId); }
            set { this.Store(x => x.MediaId, value); }
        }

        public int? Width {
            get { return this.Retrieve(x => x.Width); }
            set { this.Store(x => x.Width, value); }
        }

        public int? Height {
            get { return this.Retrieve(x => x.Height); }
            set { this.Store(x => x.Height, value); }
        }

        public int? DesignWidth
        {
            get { return this.Retrieve(x => x.DesignWidth); }
            set { this.Store(x => x.DesignWidth, value); }
        }

        public int? DesignHeight
        {
            get { return this.Retrieve(x => x.DesignHeight); }
            set { this.Store(x => x.DesignHeight, value); }
        }
    }
}