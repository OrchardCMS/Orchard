using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class FlavoredPartHandler : ContentHandler {
        public FlavoredPartHandler() {
            OnGetDisplayShape<FlavoredPart>((ctx, part) => ctx.Shape.Zones["Main"].Add(part));
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<FlavoredPart>();
            }
        }
    }
}
