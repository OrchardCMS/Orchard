using Orchard.ContentManagement;

namespace Orchard.Themes.Models {
    public class ThemeSiteSettings : ContentPart<ThemeSiteSettingsRecord> {
        public string CurrentThemeName {
            get { return Record.CurrentThemeName; }
            set { Record.CurrentThemeName = value; }
        }
    }
}