using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentItemDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType) {
            return ContentItemTemplate("Items/Contents.Item").LongestMatch(displayType, "Summary", "SummaryAdmin");
        }
    }
}