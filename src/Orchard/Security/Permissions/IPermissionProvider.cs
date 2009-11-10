using System.Collections.Generic;

namespace Orchard.Security.Permissions {
    public interface IPermissionProvider {
        string PackageName { get; }
        IEnumerable<Permission> GetPermissions();
    }
}