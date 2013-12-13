using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Templates.Models;
using Orchard.Templates.Services;

namespace Orchard.Templates.Handlers {
    public class ShapePartHandler : ContentHandler {
        public ShapePartHandler(IRepository<ShapePartRecord> repository, ISignals signals) {
            Filters.Add(StorageFilter.For(repository));

            OnGetContentItemMetadata<ShapePart>((ctx, part) => ctx.Metadata.DisplayText = part.Name);
            OnCreated<ShapePart>((ctx, part) => signals.Trigger(DefaultTemplateService.TemplatesSignal));
        }
    }
}