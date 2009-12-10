using Orchard.Models;

namespace Orchard.Themes {
    /// <summary>
    /// Interface provided by the "themes" model. 
    /// </summary>
    public interface ITheme : IContent {
        string ThemeName { get; set; }
    }
}
