using Orchard.Core.Common.Settings;

namespace Orchard.Core.Common.ViewModels {
    public class TextFieldSettingsEventsViewModel {
        public TextFieldSettingsEventsViewModel() {
            Flavors = new string[0];
        }

        public TextFieldSettings Settings { get; set; }
        public string[] Flavors { get; set; }
    }
}