using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    public class ClientIpAddressSettingsPartDriver : ContentPartDriver<ClientIpAddressSettingsPart> {
        private readonly IAuthorizer _authorizer;

        public ClientIpAddressSettingsPartDriver(IAuthorizer authorizer) {
            _authorizer = authorizer;
        }

        protected override DriverResult Editor(ClientIpAddressSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ClientIpAddressSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageClientIpAddressSettings))
                return null;

            return ContentShape("Parts_ClientIpAddressSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.ClientIpAddressSettings", Model: part, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}