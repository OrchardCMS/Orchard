using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Services;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Models {
    [UsedImplicitly]
    public class HasCommentsHandler : ContentHandler {
        public HasCommentsHandler(
            IRepository<Comment> commentsRepository,
            IRepository<HasCommentsRecord> hasCommentsRepository,
            ICommentService commentService) {

            Filters.Add(new ActivatingFilter<HasComments>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasComments>("blogpost"));
            Filters.Add(new StorageFilter<HasCommentsRecord>(hasCommentsRepository));

            OnActivated<HasComments>((ctx, x) => {
                x.CommentsActive = true;
                x.CommentsShown = true;
            });

            OnLoading<HasComments>((context, comments) => {
                comments.Comments = commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Approved).ToList();
                comments.PendingComments = commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Pending).ToList();
            });

            OnRemoved<HasComments>(
                (context, c) =>
                commentService.GetCommentsForCommentedContent(context.ContentItem.Id).ToList().ForEach(
                    commentsRepository.Delete));
        }
    }
#if false
    [UsedImplicitly]
    public class HasCommentsContainerHandler : ContentHandler {
        public HasCommentsContainerHandler() {
            Filters.Add(new ActivatingFilter<HasCommentsContainer>("blog"));
        }
    }

    public class HasCommentsContainer : ContentPart {
    }

    public class HasCommentsContainerDriver : ContentPartDriver<HasCommentsContainer> {
        protected override DriverResult Display(HasCommentsContainer part, string displayType) {
            if (displayType.Contains("Summary")) {
                // Find all contents item with this part as the container
                var parts = part.ContentItem.ContentManager.Query()
                    .Where<CommonRecord>(rec => rec.Container == part.ContentItem.Record).List();

                // Count comments and create template
                int count = parts.Aggregate(0, (seed, item) => item.Has<HasComments>() ? item.As<HasComments>().CommentCount : 0);

                var model = new CommentCountViewModel { Item = part.ContentItem, CommentCount = count };
                return ContentPartTemplate(model, "Parts/Comments.Count").Location("meta");
            }

            return null;
        }
    }
#endif
}
