using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Users.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    [UsedImplicitly]
    public class UserPartHandler : ContentHandler {
        public UserPartHandler(IRepository<UserPartRecord> repository) {
            Filters.Add(new ActivatingFilter<UserPart>(UserPartDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}