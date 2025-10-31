using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Canvas : Container
    {

        public override string Category => "Layout";

        public override LocalizedString DisplayText => T("Canvas");

        public override bool IsSystemElement => true;

        public override bool HasEditor => false;
    }
}