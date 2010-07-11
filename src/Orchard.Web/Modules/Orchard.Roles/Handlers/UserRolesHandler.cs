using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Roles.Models;

namespace Orchard.Roles.Handlers {
    [UsedImplicitly]
    public class UserRolesHandler : ContentHandler {
        private readonly IRepository<UserRolesRecord> _userRolesRepository;

        public UserRolesHandler(IRepository<UserRolesRecord> userRolesRepository) {
            _userRolesRepository = userRolesRepository;

            Filters.Add(new ActivatingFilter<UserRoles>("User"));
            OnLoaded<UserRoles>((context, userRoles) => {
                                    userRoles.Roles = _userRolesRepository
                                        .Fetch(x => x.UserId == context.ContentItem.Id)
                                        .Select(x => x.Role.Name).ToList();
                                });
        }

    }
}