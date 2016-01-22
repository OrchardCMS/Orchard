using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentsPartHandler : ContentHandler {
        public CommentsPartHandler(
            IContentManager contentManager,
            IRepository<CommentsPartRecord> commentsRepository,
            ICommentService commentService) {

            Filters.Add(StorageFilter.For(commentsRepository));

            OnInitializing<CommentsPart>((ctx, part) => {
                part.CommentsActive = true;
                part.CommentsShown = true;
                part.Comments = new List<CommentPart>();
            });

            OnLoading<CommentsPart>((context, comments) => {
                comments.CommentsField.Loader(list =>
                    commentService.GetCommentsForCommentedContent(context.ContentItem.Id)
                    .Where(x => x.Status == CommentStatus.Approved)
                    .OrderBy(x => x.Position)
                    .List().ToList());

                comments.PendingCommentsField.Loader(list => 
                    commentService.GetCommentsForCommentedContent(context.ContentItem.Id)
                    .Where(x => x.Status == CommentStatus.Pending)
                    .List().ToList());
            });

            OnRemoved<CommentsPart>(
                (context, c) => {
                    var comments = commentService.GetCommentsForCommentedContent(context.ContentItem.Id).List();
                    foreach (var comment in comments) {
                        contentManager.Remove(comment.ContentItem);
                    }
                });
        }
    }
}