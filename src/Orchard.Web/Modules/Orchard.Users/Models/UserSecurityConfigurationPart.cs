using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class UserSecurityConfigurationPart : ContentPart<UserSecurityConfigurationPartRecord> {
        public bool SaveFromSuspension {
            get { return Retrieve(x => x.SaveFromSuspension); }
            set { Store(x => x.SaveFromSuspension, value); }
        }

        public bool PreventPasswordExpiration {
            get { return Retrieve(x => x.PreventPasswordExpiration); }
            set { Store(x => x.PreventPasswordExpiration, value); }
        }
    }
}