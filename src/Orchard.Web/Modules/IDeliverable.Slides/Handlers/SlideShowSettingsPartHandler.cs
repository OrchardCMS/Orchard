using IDeliverable.Slides.Models;
using Orchard.ContentManagement.Handlers;

namespace IDeliverable.Slides.Handlers {
    public class SlideshowSettingsPartHandler : ContentHandler
    {
        public SlideshowSettingsPartHandler()
        {
            Filters.Add(new ActivatingFilter<SlideshowSettingsPart>("Site"));
        }
    }
}