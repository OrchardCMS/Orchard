using Orchard.ContentManagement;

namespace Orchard.Layouts.Framework.Drivers {
    public interface IContentImportSession {
        ContentItem GetItemFromSession(string id);
    }
}