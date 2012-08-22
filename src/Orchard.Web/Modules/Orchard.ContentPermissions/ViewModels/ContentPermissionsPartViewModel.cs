using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentPermissions.ViewModels {
    public class ContentPermissionsPartViewModel {
        public bool Enabled { get; set; }
        
        // list of available roles
        public IList<RoleEntry> AllRoles { get; set; }
        
        public IList<RoleEntry> ViewRoles { get; set; }
        public IList<RoleEntry> ViewOwnRoles { get; set; }
        public IList<RoleEntry> PublishRoles { get; set; }
        public IList<RoleEntry> PublishOwnRoles { get; set; }
        public IList<RoleEntry> EditRoles { get; set; }
        public IList<RoleEntry> EditOwnRoles { get; set; }
        public IList<RoleEntry> DeleteRoles { get; set; }
        public IList<RoleEntry> DeleteOwnRoles { get; set; }

        public static IList<RoleEntry> ExtractRoleEntries(IEnumerable<string> allRoles, string allowed) {
            if(String.IsNullOrWhiteSpace(allowed)) {
                allowed = String.Empty;
            }

            var allowedRoles = allowed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return allRoles.OrderBy(x => x).Select(x => new RoleEntry { Role = x, Checked = allowedRoles.Contains(x, StringComparer.OrdinalIgnoreCase) }).ToList();
        }

        public static string SerializePermissions(IEnumerable<RoleEntry> roleEntries) {
            return String.Join(",", roleEntries.Where(x => x.Checked).Select(x => x.Role).ToArray());
        }
    }

    public class RoleEntry {
        public string Role { get; set; }
        public bool Default { get; set; }
        public bool Checked { get; set; }
        public bool Enabled { get; set; }
    }
}