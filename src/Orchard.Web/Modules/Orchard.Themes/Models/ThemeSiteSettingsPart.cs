using Orchard.ContentManagement;

namespace Orchard.Themes.Models {
    public class ThemeSiteSettingsPart : ContentPart {
        public string CurrentThemeName {
            get { return this.Retrieve(x => x.CurrentThemeName); }
            set { this.Store(x => x.CurrentThemeName, value); }
        }
        public string CurrentAdminThemeName
        {
            get { return this.Retrieve(x => x.CurrentAdminThemeName); }
            set { this.Store(x => x.CurrentAdminThemeName, value); }
        }
    }
}