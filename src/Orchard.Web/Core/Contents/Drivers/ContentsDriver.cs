using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.ViewModels;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentItemDriver<ContentPart> {
        protected override string GetDisplayText(ContentPart item) {
            return item.Is<IRoutableAspect>()
                       ? item.As<IRoutableAspect>().Title
                       : base.GetDisplayText(item);
        }

        protected override DriverResult Display(ContentPart part, string displayType) {
            var location = part.GetLocation(displayType, "secondary", null);
            return Combined(
                ContentItemTemplate("Items/Contents.Item").LongestMatch(displayType, "Summary", "SummaryAdmin"),
                ContentPartTemplate(new PublishContentViewModel(part.ContentItem), "Parts/Contents.Publish").LongestMatch(displayType, "Summary", "SummaryAdmin").Location(location));
        }
    }
}