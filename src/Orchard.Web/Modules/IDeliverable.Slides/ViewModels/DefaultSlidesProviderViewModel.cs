using System.Collections.Generic;
using IDeliverable.Slides.Elements;
using IDeliverable.Slides.Models;

namespace IDeliverable.Slides.ViewModels
{
    public class DefaultSlidesProviderViewModel
    {
        public DefaultSlidesProviderViewModel()
        {
            Slides = new List<dynamic>();
            Indices = new List<int>();
        }

        public IList<dynamic> Slides { get; set; }
        public ISlideshow Slideshow { get; set; }
        public string SessionKey { get; set; }
        public IList<int> Indices { get; set; }
        public string SlidesData { get; set; }
    }
}