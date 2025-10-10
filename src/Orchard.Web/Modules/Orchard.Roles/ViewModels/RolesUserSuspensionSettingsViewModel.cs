using System.Collections.Generic;
using Newtonsoft.Json;

namespace Orchard.Roles.ViewModels {
    public class RolesUserSuspensionSettingsViewModel {

        public RolesUserSuspensionSettingsViewModel() {
            Configuration = new List<RoleSuspensionConfiguration>();
        }

        public List<RoleSuspensionConfiguration> Configuration { get; set; }
    }

    public class RoleSuspensionConfiguration {
        public int RoleId { get; set; }
        [JsonIgnore]
        public string RoleName { get; set; }
        [JsonIgnore]
        public string RoleLabel { get; set; }
        public bool IsSafeFromSuspension { get; set; }
    }
}