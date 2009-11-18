using Orchard.Mvc.ViewModels;
using Orchard.Core.Settings.Models;

namespace Orchard.Core.Settings.ViewModels {
    public class SettingsIndexViewModel : AdminViewModel {
        public SiteModel SiteSettings { get; set; }
    }
}
