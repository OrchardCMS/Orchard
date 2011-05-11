using Orchard.Comments.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Comments.Handlers {
    public class CommentPartHandler : ContentHandler {
        public CommentPartHandler(IRepository<CommentPartRecord> commentsRepository) {
            Filters.Add(StorageFilter.For(commentsRepository));
        }
    }
}