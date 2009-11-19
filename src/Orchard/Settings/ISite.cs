using Orchard.Models;

namespace Orchard.Settings {
    /// <summary>
    /// Interface provided by the "settings" model. 
    /// </summary>
    public interface ISite : IContentItemPart {
        string SiteName { get; }
        string SuperUser { get; }
    }
}
