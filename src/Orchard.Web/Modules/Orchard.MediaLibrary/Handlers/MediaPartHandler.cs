using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;
using Orchard.Taxonomies.Models;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public MediaPartHandler(
            IContentManager contentManager, 
            IRepository<MediaPartRecord> repository, 
            IStorageProvider storageProvider) {
            
            _contentManager = contentManager;
            _storageProvider = storageProvider;

            Filters.Add(StorageFilter.For(repository));
            OnLoading<MediaPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<MediaPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnRemoving<MediaPart>((context, part) => RemoveMedia(part));
        }

        protected void LazyLoadHandlers(MediaPart part) {
            // add handlers that will load content for id's just-in-time
            part.TermPartField.Loader(() => part.Record.TermPartRecord == null ? null : _contentManager.Get<TermPart>(part.Record.TermPartRecord.Id));
        }

        protected void RemoveMedia(MediaPart part) {
            if (part.Resource.StartsWith("~/")) {
                var path = _storageProvider.GetLocalPath(part.Resource);
                _storageProvider.DeleteFile(path);
            }
        }
    }
}