using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Tokens.Models;

namespace Orchard.Tokens.Drivers {
    [OrchardFeature("Orchard.Tokens.Feeds")]
    public class RssPartDriver : ContentPartDriver<RssPart> {
    }
}