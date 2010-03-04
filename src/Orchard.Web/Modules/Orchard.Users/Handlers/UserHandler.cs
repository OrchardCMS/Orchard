using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    [UsedImplicitly]
    public class UserHandler : ContentHandler {
        public UserHandler(IRepository<UserRecord> repository) {
            Filters.Add(new ActivatingFilter<User>(UserDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}