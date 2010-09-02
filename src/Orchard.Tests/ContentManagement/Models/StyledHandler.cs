using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Tests.ContentManagement.Models {
    public class StyledHandler : ContentHandler {
        public StyledHandler() {
            OnGetDisplayShape<Styled>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part) { Position = "10" }));
        }

        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Styled>();
            }
        }
    }
}
