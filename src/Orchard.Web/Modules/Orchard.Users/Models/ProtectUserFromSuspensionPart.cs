using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    public class ProtectUserFromSuspensionPart : ContentPart<ProtectUserFromSuspensionPartRecord> {
        public bool SaveFromSuspension {
            get { return Retrieve(x => x.SaveFromSuspension); }
            set { Store(x => x.SaveFromSuspension, value); }
        }
    }
}