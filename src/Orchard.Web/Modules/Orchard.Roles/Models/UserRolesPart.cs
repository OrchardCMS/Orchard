using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Roles.Models
{
    public class UserRolesPart : ContentPart, IUserRoles
    {

        internal LazyField<IList<string>> _roles = new LazyField<IList<string>>();

        public IList<string> Roles => _roles.Value;
    }
}