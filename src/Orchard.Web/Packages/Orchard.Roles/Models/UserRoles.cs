using System.Collections.Generic;
using Orchard.Models;

namespace Orchard.Roles.Models.NoRecord {
    public interface IUserRoles : IContent {
        IList<string> Roles { get; }
    }

    public class UserRoles : ContentPart, IUserRoles {
        public UserRoles() {
            Roles = new List<string>();
        }

        public IList<string> Roles { get; set; }
    }
}