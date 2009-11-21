using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserProvider : ContentProvider {
        public UserProvider(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>("user"));
            Filters.Add(new StorageFilterForRecord<UserRecord>(repository));
        }
    }
}
