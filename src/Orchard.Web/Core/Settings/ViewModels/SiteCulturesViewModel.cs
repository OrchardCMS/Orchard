using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Settings.ViewModels {
    public class SiteCulturesViewModel : BaseViewModel {
        public string CurrentCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IEnumerable<string> AvailableSystemCultures { get; set; }
    }
}