using Orchard.ContentManagement.Handlers;

namespace Orchard.Tests.ContentManagement.Models {
    public class StyledHandler : ContentHandler {
        public StyledHandler() {
            OnGetDisplayShape<StyledPart>((ctx, part) => ctx.Shape.Zones["Main"].Add(part, "10"));
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<StyledPart>();
            }
        }
    }
}
