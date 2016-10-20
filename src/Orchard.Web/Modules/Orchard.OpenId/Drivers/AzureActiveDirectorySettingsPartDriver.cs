using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Drivers {
    public class AzureActiveDirectorySettingsPartDriver : ContentPartDriver<AzureActiveDirectorySettingsPart> {

        protected override DriverResult Editor(AzureActiveDirectorySettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AzureActiveDirectorySettings_Edit", () =>
               shapeHelper.EditorTemplate(TemplateName: "Parts.AzureActiveDirectorySettings", Model: part, Prefix: Prefix)).OnGroup("Azure AD Authentication");
        }

        protected override DriverResult Editor(AzureActiveDirectorySettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}