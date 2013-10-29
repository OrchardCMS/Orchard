using Orchard.ContentManagement;

namespace Orchard.Themes.Models {
    public class ThemeSiteSettingsPart : ContentPart {
        public string CurrentThemeName {
            get { return Get("CurrentThemeName"); }
            set { Set("CurrentThemeName", value); }
        }
    }
}