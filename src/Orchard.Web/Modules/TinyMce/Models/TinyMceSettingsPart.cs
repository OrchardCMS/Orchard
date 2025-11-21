using Orchard.ContentManagement;

namespace TinyMce.Models
{
    public class TinyMceSettingsPart : ContentPart
    {
        public string ValidElements
        {
            get { return this.Retrieve(x => x.ValidElements); }
            set { this.Store(x => x.ValidElements, value); }
        }
    }
}
