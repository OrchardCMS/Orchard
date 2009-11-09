using System.Collections.Generic;

namespace Orchard.Security {
    public interface IPermissionProvider {
        string PackageName { get; }
        IEnumerable<Permission> GetPermissions();
    }
}
