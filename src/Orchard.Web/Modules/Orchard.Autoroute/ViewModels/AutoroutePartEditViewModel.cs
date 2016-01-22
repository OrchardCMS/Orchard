using Orchard.Autoroute.Settings;

namespace Orchard.Autoroute.ViewModels {

    public class AutoroutePartEditViewModel {
        public AutorouteSettings Settings { get; set; }
        public bool IsHomePage { get; set; }
        public bool PromoteToHomePage { get; set; }
        public string CurrentUrl { get; set; }
        public string CustomPattern { get; set; }
    }
}
