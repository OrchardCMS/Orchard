using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Orchard.Comments.Drivers {
    [UsedImplicitly]
    public class HasCommentsContainerDriver : ContentPartDriver<HasCommentsContainer> {
        protected override DriverResult Display(HasCommentsContainer part, string displayType) {
            if (displayType == "SummaryAdmin") {
                return ContentPartTemplate(CreateViewModel(part.ContentItem), "Parts/Comments.CountAdmin").Location("meta");
            }
            else if (displayType.Contains("Summary")) {
                return ContentPartTemplate(CreateViewModel(part.ContentItem), "Parts/Comments.Count").Location("meta");
            }

            return null;
        }

        private static CommentCountViewModel CreateViewModel(ContentItem contentItem) {
            // Find all contents item with this part as the container
            var parts = contentItem.ContentManager.Query()
                .Where<CommonRecord>(rec => rec.Container == contentItem.Record).List();

            // Count comments and create template
            int count = parts.Aggregate(0, (seed, item) => seed + (item.Has<HasComments>() ? item.As<HasComments>().Comments.Count : 0));
            int pendingCount = parts.Aggregate(0, (seed, item) => seed + (item.Has<HasComments>() ? item.As<HasComments>().PendingComments.Count : 0));

            return new CommentCountViewModel { Item = contentItem, CommentCount = count, PendingCount = pendingCount};
        }
    }
}