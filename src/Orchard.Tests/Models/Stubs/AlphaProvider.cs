using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class AlphaProvider : ContentProvider {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
