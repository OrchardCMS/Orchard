using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Users.Models;

namespace Orchard.Users.Drivers {
    public class UserSecurityConfigurationPartDriver : ContentPartDriver<UserSecurityConfigurationPart> {

        public UserSecurityConfigurationPartDriver() { }

        protected override DriverResult Editor(
            UserSecurityConfigurationPart part, dynamic shapeHelper) {

            return ContentShape("Parts_User_UserSecurityConfiguration_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/User.UserSecurityConfiguration",
                    Model: part,
                    Prefix: Prefix
                    ));
        }
        protected override DriverResult Editor(
            UserSecurityConfigurationPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

    }
}