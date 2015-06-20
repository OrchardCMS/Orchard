using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace IDeliverable.Slides.Models
{
    public class SlideshowPart : ContentPart, ISlideshow
    {
        internal readonly LazyField<SlideshowProfile> _profileField = new LazyField<SlideshowProfile>();

        /// <summary>
        /// The player profile ID to use when presenting the slides.
        /// </summary>
        public int? ProfileId
        {
            get { return this.Retrieve(x => x.ProfileId, versioned: true); }
            set { this.Store(x => x.ProfileId, value, versioned: true); }
        }

        /// <summary>
        /// The player profile ID to use when presenting the slides.
        /// </summary>
        public SlideshowProfile Profile => _profileField.Value;

        /// <summary>
        /// The name of the slides provider that provides the slides.
        /// </summary>
        public string ProviderName
        {
            get { return this.Retrieve(x => x.ProviderName, versioned: true); }
            set { this.Store(x => x.ProviderName, value, versioned: true); }
        }
    }
}