using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IDeliverable.Slides.Services;

namespace IDeliverable.Slides.ViewModels
{
    public class SlideshowProfileViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SelectedEngine { get; set; }
        public IList<ISlideshowPlayerEngine> AvailableEngines { get; set; }
        public IDictionary<string, dynamic> EngineSettingsEditors { get; set; }
    }
}