using System.Collections.Generic;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Roles.ViewModels;

namespace Orchard.Roles.Models {
    public class RolesUserSuspensionSettingsPart : ContentPart {
        public List<RoleSuspensionConfiguration> Configuration {
            get {
                return JsonConvert
                  .DeserializeObject<List<RoleSuspensionConfiguration>>(SerializedConfiguration);
            }
            set { SerializedConfiguration = JsonConvert.SerializeObject(value); }
        }

        public string SerializedConfiguration {
            get { return this.Retrieve(x => x.SerializedConfiguration, defaultValue: string.Empty); }
            set { this.Store(x => x.SerializedConfiguration, value ?? string.Empty); }
        }
    }
}