using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Roles.Models {
    public class UserRolesPart : ContentPart, IUserRoles {
        public UserRolesPart() {
            Roles = new List<string>();
        }

        public IList<string> Roles { get; set; }
    }
}