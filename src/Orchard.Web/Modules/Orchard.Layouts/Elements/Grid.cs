using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Grid : Container
    {
        public const int GridSize = 12;

        public override string Category => "Layout";

        public override LocalizedString DisplayText => T("Grid");

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;
    }
}