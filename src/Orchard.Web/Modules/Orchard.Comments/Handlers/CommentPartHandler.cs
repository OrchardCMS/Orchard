using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentPartHandler : ContentHandler {
        public CommentPartHandler(IRepository<CommentPartRecord> commentsRepository) {
            Filters.Add(StorageFilter.For(commentsRepository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<CommentPart>();

            if (part != null) {
                context.Metadata.Identity.Add("Comment.CommentAuthor", part.Record.Author);
            }
        }
    }
}