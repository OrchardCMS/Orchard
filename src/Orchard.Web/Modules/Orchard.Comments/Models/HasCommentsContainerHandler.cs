using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Models {
    [UsedImplicitly]
    public class HasCommentsContainerHandler : ContentHandler {
        public HasCommentsContainerHandler() {
            Filters.Add(new ActivatingFilter<HasCommentsContainer>("blog"));
        }
    }
}