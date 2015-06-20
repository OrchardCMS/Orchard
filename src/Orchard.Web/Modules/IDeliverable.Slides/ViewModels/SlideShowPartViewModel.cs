using System.Collections.Generic;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideshowPartViewModel
    {
        public SlideshowPartViewModel()
        {
            AvailableProfiles = new List<SlideshowProfile>();
            AvailableProviders = new Dictionary<string, dynamic>();
        }

        public SlideshowPart Part { get; set; }
        public int? ProfileId { get; set; }
        public IList<SlideshowProfile> AvailableProfiles { get; set; }
        public string ProviderName { get; set; }
        public IDictionary<string, dynamic> AvailableProviders { get; set; }
    }
}