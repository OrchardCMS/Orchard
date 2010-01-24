using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Controllers;

namespace Orchard.Users.Models {
    public class UserHandler : ContentHandler {
        public UserHandler(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>(UserDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
