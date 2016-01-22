using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.ContentManagement;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentPartHandler : ContentHandler {
        public CommentPartHandler(
            IRepository<CommentPartRecord> commentsRepository,
            IContentManager contentManager,
            ICommentService commentService
            ) {

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

            // keep CommentsPart.Count in sync
            OnPublished<CommentPart>((context, part) => commentService.ProcessCommentsCount(part.CommentedOn));
            OnUnpublished<CommentPart>((context, part) => commentService.ProcessCommentsCount(part.CommentedOn));
            OnVersioned<CommentPart>((context, part, newVersionPart) => commentService.ProcessCommentsCount(newVersionPart.CommentedOn));
            OnRemoved<CommentPart>((context, part) => commentService.ProcessCommentsCount(part.CommentedOn));

            OnIndexing<CommentPart>((context, commentPart) => context.DocumentIndex
                                                                .Add("commentText", commentPart.Record.CommentText));
        }

    }


}