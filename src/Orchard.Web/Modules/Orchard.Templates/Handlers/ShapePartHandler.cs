using Orchard.Compilation.Razor;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Templates.Models;

namespace Orchard.Templates.Handlers {
    public class ShapePartHandler : ContentHandler {
        private readonly IRazorTemplateHolder _razorTemplateHolder;

        public ShapePartHandler(
            IRepository<ShapePartRecord> repository, 
            IRazorTemplateHolder razorTemplateHolder) {
            _razorTemplateHolder = razorTemplateHolder;
            Filters.Add(StorageFilter.For(repository));

            OnGetContentItemMetadata<ShapePart>((ctx, part) => ctx.Metadata.DisplayText = part.Name);
            OnUpdated<ShapePart>((ctx, part) => _razorTemplateHolder.Set(part.Name, part.Template));
        }
    }
}