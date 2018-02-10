using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class BetaPartHandler : ContentHandler {
        public BetaPartHandler() {
            Filters.Add(new ActivatingFilter<BetaPart>("beta"));
        }
    }
}
