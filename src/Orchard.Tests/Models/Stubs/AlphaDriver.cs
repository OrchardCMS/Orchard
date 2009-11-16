using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class AlphaDriver : ModelDriver {
        protected override void New(NewModelContext context) {
            if (context.ModelType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
