using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Widgets.Models;

namespace Orchard.Widgets.Handlers {
    [UsedImplicitly]
    public class LayerPartHandler : ContentHandler {
        public LayerPartHandler(IRepository<LayerPartRecord> layersRepository) {
            Filters.Add(StorageFilter.For(layersRepository));
        }
    }
}