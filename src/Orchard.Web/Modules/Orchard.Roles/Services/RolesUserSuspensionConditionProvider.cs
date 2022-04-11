using System.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Settings;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Roles.Services {
    public class RolesUserSuspensionConditionProvider : IUserSuspensionConditionProvider {
        private readonly ISiteService _siteService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;

        public RolesUserSuspensionConditionProvider(
            ISiteService siteService,
            IRepository<UserRolesPartRecord> userRolesRepository) {

            _siteService = siteService;
            _userRolesRepository = userRolesRepository;
        }

        public IContentQuery<UserPart> AlterQuery(IContentQuery<UserPart> query) {
            return query;
        }

        public bool UserIsProtected(UserPart userPart) {
            // Get the user roles: we fetch them directly from the repository rather
            // than through a part, because we are going to need the roles Ids and
            // not just their names.
            var roleIds = _userRolesRepository
                .Fetch(urpr => urpr.UserId == userPart.Id)
                .Select(urpr => urpr.Role.Id)
                .ToList();
            // get settings
            var safeRoleIds = Settings.Configuration
                .Where(rsc => rsc.IsSafeFromSuspension)
                .Select(rsc => rsc.RoleId);
            // Case where we are "saving" users with no specific assigned role (i.e.
            // these users are just Authenticated)
            if (safeRoleIds.Contains(0) && !roleIds.Any()) {
                return true;
            }
            // If the user has assigned roles we need to check whether any of those
            // makes them "safe".
            return roleIds.Any(i => safeRoleIds.Contains(i));
        }

        private RolesUserSuspensionSettingsPart _settingsPart;
        private RolesUserSuspensionSettingsPart Settings {
            get {
                if (_settingsPart == null) {
                    _settingsPart = _siteService.GetSiteSettings().As<RolesUserSuspensionSettingsPart>();
                }
                return _settingsPart;
            }
        }
    }
}