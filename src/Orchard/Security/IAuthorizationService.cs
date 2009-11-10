using Orchard.Security.Permissions;

namespace Orchard.Security {
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system
    /// provided by default. 
    /// </summary>
    public interface IAuthorizationService : IDependency {
        bool CheckAccess(IUser user, Permission permission);
    }
}
