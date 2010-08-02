using Orchard.ContentManagement.Records;

namespace Orchard.Themes.Models {
    public class ThemeSiteSettingsPartRecord : ContentPartRecord {
        public virtual string CurrentThemeName { get; set; }
    }
}