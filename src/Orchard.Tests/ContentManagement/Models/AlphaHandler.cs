using Orchard.ContentManagement.Handlers;

namespace Orchard.Tests.ContentManagement.Models {
    public class AlphaHandler : ContentHandler {
        public AlphaHandler() {
            OnGetDisplayShape<Alpha>((ctx, part) => ctx.Shape.Zones["Main"].Add(part, "3"));
        }
        
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
