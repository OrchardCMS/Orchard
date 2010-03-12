using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Roles.Models {
    public interface IUserRoles : IContent {
        IList<string> Roles { get; }
    }
}