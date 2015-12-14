using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.MediaProcessing.Models;

namespace Orchard.MediaProcessing.Handlers {
    public class ImageProfilePartHandler : ContentHandler {
        public ImageProfilePartHandler(IRepository<ImageProfilePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}