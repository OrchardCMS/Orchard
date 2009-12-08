using System.Collections.Generic;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserHandler : ContentHandler {
        public override IEnumerable<ContentType> GetContentTypes() {
            return new[] { User.ContentType };
        }

        public UserHandler(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>("user"));
            Filters.Add(new StorageFilter<UserRecord>(repository));
        }
    }
}
