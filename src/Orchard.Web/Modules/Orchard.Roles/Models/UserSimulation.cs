using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Roles.Models.NoRecord;
using Orchard.Security;

namespace Orchard.Roles.Models {
    public static class UserSimulation {
        public static IUser Create(string role) {
            var simulation = new ContentItemBuilder("user")
                .Weld<SimulatedUser>()
                .Weld<SimulatedUserRoles>()
                .Build();
            simulation.As<SimulatedUserRoles>().Roles = new[] {role};
            return simulation.As<IUser>();
        }

        class SimulatedUser : ContentPart, IUser {
            public int Id { get { return ContentItem.Id; } }
            public string UserName { get { return null; } }
            public string Email { get { return null; } }
        }

        class SimulatedUserRoles : ContentPart, IUserRoles {
            public IList<string> Roles { get; set; }
        }
    }
}
