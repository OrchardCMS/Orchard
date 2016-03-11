using Orchard.Autoroute.Settings;
using System.Collections.Generic;

namespace Orchard.Autoroute.ViewModels {

    public class AutoroutePartEditViewModel {
        public AutorouteSettings Settings { get; set; }
        public bool IsHomePage { get; set; }
        public bool PromoteToHomePage { get; set; }
        public string CurrentUrl { get; set; }
        public string CustomPattern { get; set; }
        public string CurrentCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
    }
}
