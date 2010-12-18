using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class AlphaPartHandler : ContentHandler {
        public AlphaPartHandler() {
            OnGetDisplayShape<AlphaPart>((ctx, part) => ctx.Shape.Zones["Main"].Add(part, "3"));
        }
        
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<AlphaPart>();
            }
        }
    }
}
