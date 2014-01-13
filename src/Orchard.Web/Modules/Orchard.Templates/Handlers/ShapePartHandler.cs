using Orchard.Caching;
using Orchard.Compilation.Razor;
using Orchard.ContentManagement.Handlers;
using Orchard.Templates.Models;
using Orchard.Templates.Services;

namespace Orchard.Templates.Handlers {
    public class ShapePartHandler : ContentHandler {
        private readonly IRazorTemplateHolder _razorTemplateHolder;

        public ShapePartHandler(ISignals signals, IRazorTemplateHolder razorTemplateHolder) {
            _razorTemplateHolder = razorTemplateHolder;

            OnGetContentItemMetadata<ShapePart>((ctx, part) => ctx.Metadata.DisplayText = part.Name);
            OnUpdated<ShapePart>((ctx, part) => _razorTemplateHolder.Set(part.Name, part.Template));
            OnCreated<ShapePart>((ctx, part) => signals.Trigger(DefaultTemplateService.TemplatesSignal));
            OnRemoved<ShapePart>((ctx, part) => signals.Trigger(DefaultTemplateService.TemplatesSignal));
        }
    }
}