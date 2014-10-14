using Orchard.Layouts.Framework.Elements;
using Orchard.Localization;

namespace Orchard.Layouts.Elements {
    public class Grid : Container, IGrid {
        public const int GridSize = 12;

        public override string Category {
            get { return "Layout"; }
        }

        public override LocalizedString DisplayText {
            get { return T("Grid"); }
        }
    }
}