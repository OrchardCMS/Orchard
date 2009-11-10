using Orchard.Models.Driver;

namespace Orchard.Tests.Models.Stubs {
    public class FlavoredDriver : ModelDriver {
        protected override void New(NewModelContext context) {
            if (context.ModelType == "beta" || context.ModelType == "alpha") {
                WeldModelPart<Flavored>(context);
            }
        }
    }
}
