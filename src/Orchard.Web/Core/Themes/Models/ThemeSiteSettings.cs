using Orchard.Core.Themes.Records;
using Orchard.Models;

namespace Orchard.Core.Themes.Models {
    public class ThemeSiteSettings : ContentPart<ThemeSiteSettingsRecord> {
        public string CurrentThemeName {
            get { return Record.CurrentThemeName; }
            set { Record.CurrentThemeName = value; }
        }
    }
}
