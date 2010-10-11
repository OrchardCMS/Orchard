namespace Orchard.Themes {
    /// <summary>
    /// Interface provided by the "themes" model. 
    /// </summary>
    public interface ITheme {
        string ThemeName { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string Version { get; set; }
        string Author { get; set; }
        string HomePage { get; set; }
        string Tags { get; set; }
        string Zones { get; set; }
        string BaseTheme { get; set; }
    }
}
