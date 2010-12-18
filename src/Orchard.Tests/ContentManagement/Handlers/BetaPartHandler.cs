using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class BetaPartHandler : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta") {
                context.Builder.Weld<BetaPart>();
            }
        }
    }
}
