using JetBrains.Annotations;
using Orchard.Comments.Drivers;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentHandler : ContentHandler {
        public CommentHandler(IRepository<CommentRecord> commentsRepository) {
            Filters.Add(StorageFilter.For(commentsRepository));
        }
    }
}