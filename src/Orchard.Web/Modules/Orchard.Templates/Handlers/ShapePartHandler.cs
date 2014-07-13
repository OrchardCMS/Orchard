using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Templates.Models;
using Orchard.Templates.Services;

namespace Orchard.Templates.Handlers {
    public class ShapePartHandler : ContentHandler {
        private ISignals _signals;

        public ShapePartHandler(ISignals signals) {
            _signals = signals;

            OnPublished<ShapePart>((ctx, part) => InvalidateTemplatesCache());
            OnUnpublished<ShapePart>((ctx, part) => InvalidateTemplatesCache());
            OnRemoved<ShapePart>((ctx, part) => InvalidateTemplatesCache());
        }

        public void InvalidateTemplatesCache() {
            _signals.Trigger(DefaultTemplateService.TemplatesSignal);
        }
    }
}