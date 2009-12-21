using System.Collections.Generic;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Users.Models {
    public class UserHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { User.ContentType };
        }

        public UserHandler(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>("user"));
            Filters.Add(new StorageFilter<UserRecord>(repository));
            Filters.Add(new ContentItemTemplates<User>("Items/Users.User"));
        }
    }
}
