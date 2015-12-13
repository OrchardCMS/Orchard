using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    [UsedImplicitly]
    public class LayerPartHandler : ContentHandler {
        public LayerPartHandler(IRepository<LayerPartRecord> layersRepository) {
            Filters.Add(StorageFilter.For(layersRepository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<LayerPart>();

            if (part != null) {
                 context.Metadata.Identity.Add("Layer.LayerName", part.Name);
            }
        }
    }
}