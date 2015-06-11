using IDeliverable.Slides.Models;
using Orchard.ContentManagement.Handlers;

namespace IDeliverable.Slides.Handlers {
    public class SlideShowSettingsPartHandler : ContentHandler
    {
        public SlideShowSettingsPartHandler()
        {
            Filters.Add(new ActivatingFilter<SlideShowSettingsPart>("Site"));
        }
    }
}