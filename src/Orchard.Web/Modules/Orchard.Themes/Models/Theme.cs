namespace Orchard.Themes.Models {
    public class Theme : ITheme {
        public string ThemeName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string HomePage { get; set; }
        public string Tags { get; set; }
        public string Zones { get; set; }
    }
}