using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class HasCommentsContainerHandler : ContentHandler {
        public HasCommentsContainerHandler() {
            Filters.Add(new ActivatingFilter<HasCommentsContainer>("Blog"));
        }
    }
}