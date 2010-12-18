using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Roles.Models;

namespace Orchard.Roles.Handlers {
    [UsedImplicitly]
    public class UserRolesPartHandler : ContentHandler {
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;

        public UserRolesPartHandler(IRepository<UserRolesPartRecord> userRolesRepository) {
            _userRolesRepository = userRolesRepository;

            Filters.Add(new ActivatingFilter<UserRolesPart>("User"));
            OnLoaded<UserRolesPart>((context, userRoles) => {
                                    userRoles.Roles = _userRolesRepository
                                        .Fetch(x => x.UserId == context.ContentItem.Id)
                                        .Select(x => x.Role.Name).ToList();
                                });
        }
    }
}