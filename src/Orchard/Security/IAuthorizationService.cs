using Orchard.Security.Permissions;

namespace Orchard.Security {
    public interface IAuthorizationService : IDependency {
        bool CheckAccess(IUser user, Permission permission);
    }
}
