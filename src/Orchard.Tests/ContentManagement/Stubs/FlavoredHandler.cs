using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Tests.ContentManagement.Models {
    public class FlavoredHandler : ContentHandler {
        public FlavoredHandler() {
            OnGetDisplayViewModel<Flavored>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part)));
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "beta" || context.ContentType == "alpha") {
                context.Builder.Weld<Flavored>();
            }
        }
    }
}
