using Orchard.ContentManagement.Records;

namespace Orchard.Themes.Models {
    public class ThemeRecord : ContentPartRecord {
        public virtual string ThemeName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Version { get; set; }
        public virtual string Author { get; set; }
        public virtual string HomePage { get; set; }
        public virtual string Tags { get; set; }
    }
}