using Orchard.ContentManagement.Records;

namespace Orchard.Core.Themes.Models {
    public class ThemeSiteSettingsRecord : ContentPartRecord {
        public virtual string CurrentThemeName { get; set; }
    }
}