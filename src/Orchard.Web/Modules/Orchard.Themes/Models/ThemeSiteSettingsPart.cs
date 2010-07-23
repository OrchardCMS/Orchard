using Orchard.ContentManagement;

namespace Orchard.Themes.Models {
    public class ThemeSiteSettingsPart : ContentPart<ThemeSiteSettingsPartRecord> {
        public string CurrentThemeName {
            get { return Record.CurrentThemeName; }
            set { Record.CurrentThemeName = value; }
        }
    }
}