using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsContainerPartDriver : ContentPartDriver<CommentsContainerPart> {
        protected override DriverResult Display(CommentsContainerPart part, string displayType, dynamic shapeHelper) {
            if (displayType.Contains("Summary")) {
                // Find all contents item with this part as the container
                var parts = part.ContentItem.ContentManager.Query()
                    .Where<CommonPartRecord>(rec => rec.Container == part.ContentItem.Record).List();

                // Count comments and create template
                int count = parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().Comments.Count : 0));
                int pendingCount = parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().PendingComments.Count : 0));

                if (displayType == "SummaryAdmin")
                    return ContentShape(shapeHelper.Comments_CountAdmin(ContentPart: part, CommentCount: count, PendingCount: pendingCount)).Location(part.GetLocation("SummaryAdmin"));

                return ContentShape(shapeHelper.Comments_Count(ContentPart: part, CommentCount: count, PendingCount: pendingCount)).Location(part.GetLocation("Summary"));
            }

            return null;
        }
    }
}