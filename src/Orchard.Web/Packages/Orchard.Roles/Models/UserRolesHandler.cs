using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;
using Orchard.Roles.Models.NoRecord;
using Orchard.Roles.Records;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Roles.Models {
    public class UserRolesHandler : ContentHandler {
        private readonly IRepository<UserRolesRecord> _userRolesRepository;

        public UserRolesHandler(IRepository<UserRolesRecord> userRolesRepository) {
            _userRolesRepository = userRolesRepository;

            Filters.Add(new ActivatingFilter<UserRoles>("user"));
            OnLoaded<UserRoles>((context, userRoles) => {
                userRoles.Roles = _userRolesRepository
                    .Fetch(x => x.UserId == context.ContentItem.Id)
                    .Select(x => x.Role.Name).ToList();
            });
        }

    }
}

