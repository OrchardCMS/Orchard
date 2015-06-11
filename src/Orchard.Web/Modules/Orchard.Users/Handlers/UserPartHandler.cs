using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    [UsedImplicitly]
    public class UserPartHandler : ContentHandler {
        public UserPartHandler(IRepository<UserPartRecord> repository) {
            Filters.Add(new ActivatingFilter<UserPart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<UserPart>();

            if (part != null) {
                context.Metadata.Identity.Add("User.UserName", part.UserName);
                context.Metadata.DisplayText = part.UserName;
            }
        }
    }
}