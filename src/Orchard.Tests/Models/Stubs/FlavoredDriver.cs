using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class FlavoredDriver : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}
