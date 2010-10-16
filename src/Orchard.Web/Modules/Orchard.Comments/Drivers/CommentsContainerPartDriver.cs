using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class CommentsContainerPartDriver : ContentPartDriver<CommentsContainerPart> {
        protected override DriverResult Display(CommentsContainerPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Comments_Count",
                    () => {
                        var childItems = GetChildItems(part);
                        return shapeHelper.Parts_Comments_Count(ContentPart: part, CommentCount: GetCount(childItems), PendingCount: GetPendingCount(childItems));
                    }),
                ContentShape("Parts_Comments_Count_SummaryAdmin",
                    () => {
                        var childItems = GetChildItems(part);
                        return shapeHelper.Parts_Comments_Count_SummaryAdmin(ContentPart: part, CommentCount: GetCount(childItems), PendingCount: GetPendingCount(childItems));
                    })
                );
        }

        private static IEnumerable<ContentItem> GetChildItems(CommentsContainerPart part) {
            return part.ContentItem.ContentManager.Query()
                .Where<CommonPartRecord>(rec => rec.Container == part.ContentItem.Record).List();
        }

        private static int GetPendingCount(IEnumerable<ContentItem> parts) {
            return parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().PendingComments.Count : 0));
        }

        private static int GetCount(IEnumerable<ContentItem> parts) {
            return parts.Aggregate(0, (seed, item) => seed + (item.Has<CommentsPart>() ? item.As<CommentsPart>().Comments.Count : 0));
        }
    }
}