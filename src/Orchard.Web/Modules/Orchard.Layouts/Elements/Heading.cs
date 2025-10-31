using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace Orchard.Layouts.Elements
{
    public class Heading : ContentElement
    {

        public override string Category => "Content";

        public override LocalizedString DisplayText => T("Heading h1-h6");

        public override string ToolboxIcon => "\uf1dc";

        public int Level
        {
            get { return this.Retrieve(h => h.Level); }
            set { this.Store(h => h.Level, value); }
        }
    }
}