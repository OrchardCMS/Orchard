using Orchard.Security.Permissions;

namespace Orchard.Security {
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system
    /// provided by default. 
    /// </summary>
    public interface IAuthorizationService : IDependency {
        void CheckAccess(IUser user, Permission permission);
        bool TryCheckAccess(IUser user, Permission permission);
    }
}
