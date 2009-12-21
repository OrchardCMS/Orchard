using Orchard.ContentManagement.Records;

namespace Orchard.Core.Themes.Records {
    public class ThemeSiteSettingsRecord : ContentPartRecord {
        public virtual string CurrentThemeName { get; set; }
    }
}