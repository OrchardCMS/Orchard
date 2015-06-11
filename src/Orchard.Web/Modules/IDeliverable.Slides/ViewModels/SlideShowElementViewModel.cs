using System.Collections.Generic;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideShowElementViewModel
    {
        public SlideShowElementViewModel()
        {
            AvailableProfiles = new List<SlideShowProfile>();
            AvailableProviders = new Dictionary<string, dynamic>();
        }

        public SlideShow Element { get; set; }
        public string SessionKey { get; set; }
        public int? ProfileId { get; set; }
        public IList<SlideShowProfile> AvailableProfiles { get; set; }
        public string ProviderName { get; set; }
        public IDictionary<string, dynamic> AvailableProviders { get; set; }
    }
}