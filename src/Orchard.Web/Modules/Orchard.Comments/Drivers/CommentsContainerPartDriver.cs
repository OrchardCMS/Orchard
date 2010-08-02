using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsContainerPartDriver : ContentPartDriver<CommentsContainerPart> {
        protected override DriverResult Display(CommentsContainerPart part, string displayType) {
            if (displayType == "SummaryAdmin") {
                return ContentPartTemplate(CreateViewModel(part.ContentItem), "Parts/Comments.CountAdmin").Location(part.GetLocation("SummaryAdmin"));
            }
            else if (displayType.Contains("Summary")) {
                return ContentPartTemplate(CreateViewModel(part.ContentItem), "Parts/Comments.Count").Location(part.GetLocation("Summary"));
            }

            return null;
        }

        private static CommentCountViewModel CreateViewModel(ContentItem contentItem) {
            // Find all contents item with this part as the container
            var parts = contentItem.ContentManager.Query()
                .Where<CommonPartRecord>(rec => rec.Container == contentItem.Record).List();

            // Count comments and create template
            int count = parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().Comments.Count : 0));
            int pendingCount = parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().PendingComments.Count : 0));

            return new CommentCountViewModel { Item = contentItem, CommentCount = count, PendingCount = pendingCount};
        }
    }
}