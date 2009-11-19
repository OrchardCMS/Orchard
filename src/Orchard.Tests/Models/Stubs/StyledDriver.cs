using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class StyledDriver : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Styled>();
            }
        }
    }
}
