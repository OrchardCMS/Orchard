using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class FlavoredPartHandler : ContentHandler {
        public FlavoredPartHandler() {
            Filters.Add(new ActivatingFilter<FlavoredPart>("alpha"));
            Filters.Add(new ActivatingFilter<FlavoredPart>("beta"));

            OnGetDisplayShape<FlavoredPart>((ctx, part) => ctx.Shape.Zones["Main"].Add(part));
        }
    }
}
