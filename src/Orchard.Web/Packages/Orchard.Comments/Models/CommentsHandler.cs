using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Models {
    [UsedImplicitly]
    public class HasCommentsHandler : ContentHandler {
        public HasCommentsHandler(
            IRepository<Comment> commentsRepository,
            IRepository<HasCommentsRecord> hasCommentsRepository) {

            Filters.Add(new ActivatingFilter<HasComments>("sandboxpage"));
            Filters.Add(new ActivatingFilter<HasComments>("blogpost"));
            Filters.Add(new StorageFilter<HasCommentsRecord>(hasCommentsRepository) { AutomaticallyCreateMissingRecord = true });

            OnActivated<HasComments>((ctx, x) => {
                x.CommentsActive = true;
                x.CommentsShown = true;
            });

            OnLoading<HasComments>((context, comments) => {
                comments.Comments = commentsRepository.Fetch(x => x.CommentedOn == context.ContentItem.Id && x.Status == CommentStatus.Approved);
            });
        }


    }
}
