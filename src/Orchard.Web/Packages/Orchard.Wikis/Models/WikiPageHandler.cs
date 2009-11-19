using Orchard.Core.Common.Models;
using Orchard.Models.Driver;

namespace Orchard.Wikis.Models {
    public class WikiPageHandler : ContentHandler {
        protected override void Activating(ActivatingContentContext context) {
            if (context.ContentType == "wikipage") {
                context.Builder
                    .Weld<CommonPart>()
                    .Weld<RoutablePart>()
                    .Weld<ContentPart>();
            }
        }
    }
}
