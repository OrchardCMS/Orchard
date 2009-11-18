using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class FlavoredDriver : ModelDriver {
        protected override void Activating(ActivatingModelContext context) {
            if (context.ModelType == "beta" || context.ModelType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}
