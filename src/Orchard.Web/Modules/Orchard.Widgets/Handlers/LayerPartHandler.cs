using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    public class LayerPartHandler : ContentHandler {
        private readonly ISignals _signals;
        public LayerPartHandler(
            IRepository<LayerPartRecord> layersRepository,
            ISignals signals) {

            Filters.Add(StorageFilter.For(layersRepository));
            _signals = signals;
            
            // Evict cached content when updated, removed or destroyed.
            OnUpdated<LayerPart>(
                (context, part) => Invalidate());
            OnImported<LayerPart>(
                (context, part) => Invalidate());
            OnPublished<LayerPart>(
                (context, part) => Invalidate());
            OnRemoved<LayerPart>(
                (context, part) => Invalidate());
            OnDestroyed<LayerPart>(
                (context, part) => Invalidate());
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<LayerPart>();

            if (part != null) {
                 context.Metadata.Identity.Add("Layer.LayerName", part.Name);
            }
        }

        private void Invalidate() {
            _signals.Trigger(LayerPart.AllLayersCacheEvictSignal);
        }
    }
}