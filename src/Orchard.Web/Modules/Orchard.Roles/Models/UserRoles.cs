using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Roles.Models {
    public class UserRoles : ContentPart, IUserRoles {
        public UserRoles() {
            Roles = new List<string>();
        }

        public IList<string> Roles { get; set; }
    }
}