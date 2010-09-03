using Orchard.UI;

namespace Orchard.ContentManagement {
    public interface IContent {
        ContentItem ContentItem { get; }
        IZoneCollection Zones { get; }
    }
}
