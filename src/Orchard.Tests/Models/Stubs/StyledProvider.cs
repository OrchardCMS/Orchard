using Orchard.Models.Driver;
using Orchard.UI.Models;

namespace Orchard.Tests.Models.Stubs {
    public class StyledProvider : ContentProvider {
        public StyledProvider() {
            OnGetDisplays<Styled>((ctx, part) => ctx.Displays.Add(new ModelTemplate(part) { Position = "10" }));
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Styled>();
            }
        }
    }
}
