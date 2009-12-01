using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Tests.Models.Stubs {
    public class FlavoredProvider : ContentProvider {
        public FlavoredProvider() {
            OnGetDisplays<Flavored>((ctx, part) => ctx.Displays.Add(new ModelTemplate(part)));
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}
