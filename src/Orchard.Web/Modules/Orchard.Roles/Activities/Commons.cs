using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Workflows.Models;

namespace Orchard.Roles.Activities {
    public static class Commons {
        public static IEnumerable<string> GetRoles(ActivityContext context) {
            var roles = context.GetState<string>("Roles");

            if (String.IsNullOrEmpty(roles)) {
                return Enumerable.Empty<string>();
            }

            return roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }

    }
}