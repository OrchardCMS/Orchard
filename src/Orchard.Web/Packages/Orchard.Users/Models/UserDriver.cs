using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Users.Models {
    public class UserDriver : ContentHandler {
        public UserDriver(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<UserModel>("user"));
            Filters.Add(new StorageFilterForRecord<UserRecord>(repository));
        }
    }
}
