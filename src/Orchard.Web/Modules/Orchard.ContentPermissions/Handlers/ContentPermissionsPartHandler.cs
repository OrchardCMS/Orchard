using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.ContentPermissions.Models;

namespace Orchard.ContentPermissions.Handlers {
    public class ContentPermissionsPartHandler : ContentHandler {

        public ContentPermissionsPartHandler(IRepository<ContentPermissionsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
