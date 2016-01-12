using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Azure.Authentication.Models;

namespace Orchard.Azure.Authentication.Drivers {
    public class AzureSettingsPartDriver : ContentPartDriver<AzureSettingsPart> {
        protected override string Prefix {
            get { return "AzureSettings"; }
        }

        protected override DriverResult Editor(AzureSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AzureSettings_Edit", () =>
               shapeHelper.EditorTemplate(TemplateName: "Parts/AzureSettings", Model: part, Prefix: Prefix)).OnGroup("Azure");
        }

        protected override DriverResult Editor(AzureSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}