using Orchard.ContentManagement.Handlers;

namespace Orchard.Tests.ContentManagement.Models {
    public class BetaHandler : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta") {
                context.Builder.Weld<Beta>();
            }
        }
    }
}
