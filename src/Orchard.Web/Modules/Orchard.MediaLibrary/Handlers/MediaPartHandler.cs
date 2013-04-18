using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.MediaLibrary.Models;
using Orchard.Taxonomies.Models;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;

        public MediaPartHandler(IContentManager contentManager, IRepository<MediaPartRecord> repository) {
            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            OnLoading<MediaPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<MediaPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
        }

        protected void LazyLoadHandlers(MediaPart part) {
            // add handlers that will load content for id's just-in-time
            part.TermPartField.Loader(() => part.Record.TermPartRecord == null ? null : _contentManager.Get<TermPart>(part.Record.TermPartRecord.Id));
        }
    }
}