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
            Filters.Add(new StorageFilter<HasCommentsRecord>(hasCommentsRepository) );

            OnActivated<HasComments>((ctx, x) => {
                x.CommentsActive = true;
                x.CommentsShown = true;
            });

            OnLoading<HasComments>((context, comments) => {
                comments.Comments = commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Approved);
            });

            OnRemoved<HasComments>((context, c) => {
                if (context.ContentType == "blogpost" || context.ContentType == "sandboxpage") {
                    //TODO: (erikpo) Once comments are content items, replace the following repository delete call to a content manager remove call
                    commentService.GetCommentsForCommentedContent(context.ContentItem.Id).ToList().ForEach(
                        commentsRepository.Delete);
                }
            });
        }
    }
}