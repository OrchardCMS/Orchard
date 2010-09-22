using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.ViewModels;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType) {
            var location = part.GetLocation(displayType, "secondary", null);
            return ContentPartTemplate(new PublishContentViewModel(part.ContentItem), "Parts/Contents.Publish").LongestMatch(displayType, "Summary", "SummaryAdmin").Location(location);
        }
    }
}