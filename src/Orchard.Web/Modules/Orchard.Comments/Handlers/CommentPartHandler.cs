using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.ContentManagement;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentPartHandler : ContentHandler {
        public CommentPartHandler(
            IRepository<CommentPartRecord> commentsRepository,
            IContentManager contentManager) {

            Filters.Add(StorageFilter.For(commentsRepository));

            OnLoading<CommentPart>((context, comment) => {
                comment.CommentedOnContentItemField.Loader(
                    item => contentManager.Get(comment.CommentedOn)
                );

                comment.CommentedOnContentItemMetadataField.Loader(
                    item => contentManager.GetItemMetadata(comment.CommentedOnContentItem)
                );
            });


            OnRemoving<CommentPart>((context, comment) => {
                foreach(var response in contentManager.Query<CommentPart, CommentPartRecord>().Where(x => x.RepliedOn == comment.Id).List()) {
                    contentManager.Remove(response.ContentItem);
                }
            });

            OnIndexing<CommentPart>((context, commentPart) => context.DocumentIndex
                                                                .Add("commentText", commentPart.Record.CommentText));
        }
    }
}