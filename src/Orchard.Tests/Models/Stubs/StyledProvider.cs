using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Tests.Models.Stubs {
    public class StyledProvider : ContentProvider {
        public StyledProvider() {
            OnGetDisplays<Styled>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part) { Position = "10" }));
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Styled>();
            }
        }
    }
}
