using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    public class UserSecurityConfigurationPartHandler : ContentHandler {
        public UserSecurityConfigurationPartHandler(
            IRepository<UserSecurityConfigurationPartRecord> repository) {

            Filters.Add(new ActivatingFilter<UserSecurityConfigurationPart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}