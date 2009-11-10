using System.Collections.Generic;

namespace Orchard.Security.Permissions {
    /// <summary>
    /// Implemented by packages to enumerate the types of permissions
    /// the which may be granted
    /// </summary>
    public interface IPermissionProvider {
        string PackageName { get; }
        IEnumerable<Permission> GetPermissions();
    }
}