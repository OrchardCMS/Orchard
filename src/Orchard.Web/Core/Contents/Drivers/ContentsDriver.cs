using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentItemDriver<ContentPart> {
        protected override string GetDisplayText(ContentPart item) {
            return item.Is<IRoutableAspect>()
                       ? item.As<IRoutableAspect>().Title
                       : base.GetDisplayText(item);
        }

        protected override DriverResult Display(ContentPart part, string displayType) {
            return ContentItemTemplate("Items/Contents.Item").LongestMatch(displayType, "Summary", "SummaryAdmin");
        }
    }
}