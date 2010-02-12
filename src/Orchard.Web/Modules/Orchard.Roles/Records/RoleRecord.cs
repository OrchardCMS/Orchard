using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Roles.Records {
    public class RoleRecord {
        public RoleRecord() {
            RolesPermissions = new List<RolesPermissions>();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<RolesPermissions> RolesPermissions { get; set; }
    }
}