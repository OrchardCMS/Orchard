using IDeliverable.Slides.Models;
using Orchard.ContentManagement.Utilities;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace IDeliverable.Slides.Elements
{
    public class SlideShow : Element
    {
        internal readonly LazyField<SlideShowProfile> _profileField = new LazyField<SlideShowProfile>();

        public override string Category => "Media";
        public override bool HasEditor => true;

        public int? ProfileId
        {
            get { return this.Retrieve(x => x.ProfileId); }
            set { this.Store(x => x.ProfileId, value); }
        }

        public SlideShowProfile Profile => _profileField.Value;

        public string ProviderName
        {
            get { return this.Retrieve(x => x.ProviderName); }
            set { this.Store(x => x.ProviderName, value); }
        }
    }
}