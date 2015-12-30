using Orchard.ContentManagement;
using Orchard.ContentPicker.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentPicker.Handlers {
    public class NavigationPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IRepository<ContentMenuItemPartRecord> _repository;

        public NavigationPartHandler(IContentManager contentManager, IRepository<ContentMenuItemPartRecord> repository) {
            _contentManager = contentManager;
            _repository = repository;

            OnRemoving<NavigationPart>((context, part) => RemoveContentItems(part));
            
            OnUnpublished<NavigationPart>((context, part) => UnpublishContentItems(part));
            OnPublished<NavigationPart>((context, part) => PublishContentItems(part));
        }

        public void RemoveContentItems(NavigationPart part) {
            // look for ContentMenuItemPart with this content 
            var contentMenuItemRecords = _repository.Fetch(x => x.ContentMenuItemRecord == part.ContentItem.Record);

            // delete all menu items linking to this content item
            foreach (var contentMenuItemRecord in contentMenuItemRecords) {
                // look for any version
                var contentItem = _contentManager.Get(contentMenuItemRecord.Id, VersionOptions.AllVersions);
                if (contentItem != null) {
                    _contentManager.Remove(contentItem);
                }
            }
        }

        public void UnpublishContentItems(NavigationPart part) {
            // look for ContentMenuItemPart with this content 
            var contentMenuItemRecords = _repository.Fetch(x => x.ContentMenuItemRecord == part.ContentItem.Record);

            // delete all menu items linking to this content item
            foreach (var contentMenuItemRecord in contentMenuItemRecords) {
                // look for a published version only
                var contentItem = _contentManager.Get(contentMenuItemRecord.Id);
                if (contentItem != null) {
                    _contentManager.Unpublish(contentItem);
                }
            }
        }

        public void PublishContentItems(NavigationPart part) {
            // look for ContentMenuItemPart with this content 
            var contentMenuItemRecords = _repository.Fetch(x => x.ContentMenuItemRecord == part.ContentItem.Record);

            // delete all menu items linking to this content item
            foreach (var contentMenuItemRecord in contentMenuItemRecords) {
                // even look for an unpublished version
                var contentItem = _contentManager.Get(contentMenuItemRecord.Id, VersionOptions.Latest);
                if(contentItem != null) {
                    _contentManager.Publish(contentItem);
                }
            }
        }
    }
}