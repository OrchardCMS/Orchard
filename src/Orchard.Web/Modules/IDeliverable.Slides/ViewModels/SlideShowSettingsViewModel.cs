using System.Collections.Generic;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideShowSettingsViewModel
    {
        public string Engine { get; set; }
        public IList<ISlideShowPlayerEngine> AvailableEngines { get; set; }
        public IDictionary<string, dynamic> EngineSettingsEditors { get; set; }
    }
}