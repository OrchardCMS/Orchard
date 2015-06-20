using System.Collections.Generic;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideshowElementViewModel
    {
        public SlideshowElementViewModel()
        {
            AvailableProfiles = new List<SlideshowProfile>();
            AvailableProviders = new Dictionary<string, dynamic>();
        }

        public Slideshow Element { get; set; }
        public string SessionKey { get; set; }
        public int? ProfileId { get; set; }
        public IList<SlideshowProfile> AvailableProfiles { get; set; }
        public string ProviderName { get; set; }
        public IDictionary<string, dynamic> AvailableProviders { get; set; }
    }
}