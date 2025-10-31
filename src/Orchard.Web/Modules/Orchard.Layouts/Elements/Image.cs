using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Elements
{
    public class Image : Element
    {
        public override string Category => "Media";

        public override string ToolboxIcon => "\uf1c5";

        public int? MediaId
        {
            get { return this.Retrieve(x => x.MediaId); }
            set { this.Store(x => x.MediaId, value); }
        }
    }
}