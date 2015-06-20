using System.Collections.Generic;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideshowSettingsViewModel
    {
        public string Engine { get; set; }
        public IList<ISlideshowPlayerEngine> AvailableEngines { get; set; }
        public IDictionary<string, dynamic> EngineSettingsEditors { get; set; }
    }
}