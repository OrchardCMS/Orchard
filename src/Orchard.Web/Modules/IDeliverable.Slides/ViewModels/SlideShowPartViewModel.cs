using System.Collections.Generic;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideShowPartViewModel
    {
        public SlideShowPartViewModel()
        {
            AvailableProfiles = new List<SlideShowProfile>();
            AvailableProviders = new Dictionary<string, dynamic>();
        }

        public SlideShowPart Part { get; set; }
        public int? ProfileId { get; set; }
        public IList<SlideShowProfile> AvailableProfiles { get; set; }
        public string ProviderName { get; set; }
        public IDictionary<string, dynamic> AvailableProviders { get; set; }
    }
}