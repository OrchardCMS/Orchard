using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserHandler : ContentHandler {
        public UserHandler(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>("user"));
            Filters.Add(new StorageFilterForRecord<UserRecord>(repository));
        }
    }
}
