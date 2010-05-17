using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class HasCommentsHandler : ContentHandler {
        public HasCommentsHandler(
            IContentManager contentManager,
            IRepository<HasCommentsRecord> hasCommentsRepository,
            ICommentService commentService) {

            Filters.Add(new ActivatingFilter<HasComments>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasComments>("blogpost"));
            Filters.Add(new ActivatingFilter<HasComments>("page"));
            Filters.Add(StorageFilter.For(hasCommentsRepository));

            OnActivated<HasComments>((ctx, x) => {
                x.CommentsActive = true;
                x.CommentsShown = true;
            });

            OnLoading<HasComments>((context, comments) => {
                comments._comments.Loader(list => contentManager
                    .Query<Comment, CommentRecord>()
                    .Where(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Approved)
                    .List().ToList());

                comments._pendingComments.Loader(list => contentManager
                    .Query<Comment, CommentRecord>()
                    .Where(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Pending)
                    .List().ToList());
            });

            OnRemoved<HasComments>(
                (context, c) => {
                    foreach (var comment in commentService.GetCommentsForCommentedContent(context.ContentItem.Id)) {
                        contentManager.Remove(comment.ContentItem);
                    }
                });
        }
    }
}