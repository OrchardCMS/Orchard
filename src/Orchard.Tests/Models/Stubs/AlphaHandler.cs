using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Tests.Models.Stubs {
    public class AlphaHandler : ContentHandler {
        public AlphaHandler() {
            OnGetDisplayViewModel<Alpha>((ctx, part) => ctx.AddDisplay(new TemplateViewModel(part) { Position = "3" }));
        }
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "alpha" } };
        }
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "alpha") {
                context.Builder.Weld<Alpha>();
            }
        }
    }
}
