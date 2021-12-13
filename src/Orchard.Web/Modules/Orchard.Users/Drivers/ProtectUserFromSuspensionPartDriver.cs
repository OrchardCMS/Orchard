using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Drivers {
    // [OrchardFeature("AutomatedUserModeration")]?
    public class ProtectUserFromSuspensionPartDriver : ContentPartDriver<ProtectUserFromSuspensionPart> {

        public ProtectUserFromSuspensionPartDriver() { }

        protected override DriverResult Editor(
            ProtectUserFromSuspensionPart part, dynamic shapeHelper) {

            return ContentShape("Parts_User_ProtectFromSuspension_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/User.ProtectFromSuspension",
                    Model: part,
                    Prefix: Prefix
                    ));
        }
        protected override DriverResult Editor(
            ProtectUserFromSuspensionPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}