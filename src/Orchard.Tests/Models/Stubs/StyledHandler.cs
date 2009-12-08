using Orchard.Models.Driver;
using Orchard.Models.ViewModels;

namespace Orchard.Tests.Models.Stubs {
    public class StyledHandler : ContentHandler {
        public StyledHandler() {
            OnGetDisplayViewModel<Styled>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part) { Position = "10" }));
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Styled>();
            }
        }
    }
}
