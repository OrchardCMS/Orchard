using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.Users.Models {
    // [OrchardFeature("AutomatedUserModeration")]?
    public class ProtectUserFromSuspensionPart : ContentPart<ProtectUserFromSuspensionPartRecord> {
        public bool SaveFromSuspension {
            get { return Retrieve(x => x.SaveFromSuspension); }
            set { Store(x => x.SaveFromSuspension, value); }
        }
    }
}