using System.Collections.Generic;
using Orchard.Models;

namespace Orchard.Roles.Models.NoRecord {
    public interface IUserRoles : IContentItemPart {
        IList<string> Roles { get; }
    }

    public class UserRoles : ContentItemPart, IUserRoles {
        public UserRoles() {
            Roles = new List<string>();
        }

        public IList<string> Roles { get; set; }
    }
}