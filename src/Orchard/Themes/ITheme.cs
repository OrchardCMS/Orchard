using Orchard.ContentManagement;

namespace Orchard.Themes {
    /// <summary>
    /// Interface provided by the "themes" model. 
    /// </summary>
    public interface ITheme : IContent {
        string ThemeName { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string Version { get; set; }
        string Author { get; set; }
        string HomePage { get; set; }
    }
}
