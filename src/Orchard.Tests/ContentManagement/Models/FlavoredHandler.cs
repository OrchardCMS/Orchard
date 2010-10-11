using Orchard.ContentManagement.Handlers;

namespace Orchard.Tests.ContentManagement.Models {
    public class FlavoredHandler : ContentHandler {
        public FlavoredHandler() {
            OnGetDisplayShape<Flavored>((ctx, part) => ctx.Shape.Zones["Main"].Add(part));
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}
