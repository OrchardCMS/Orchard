using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentPartHandler : ContentHandler {
        public CommentPartHandler(IRepository<CommentPartRecord> commentsRepository) {
            Filters.Add(StorageFilter.For(commentsRepository));
        }
    }
}