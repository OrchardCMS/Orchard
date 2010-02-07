using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace Orchard.Comments.Models {
    [UsedImplicitly]
    public class CommentHandler : ContentHandler {
        public CommentHandler(IRepository<CommentRecord> commentsRepository) {
            Filters.Add(new ActivatingFilter<Comment>(CommentDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(CommentDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(commentsRepository));
        }
    }
}