using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class AlphaDriver : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
