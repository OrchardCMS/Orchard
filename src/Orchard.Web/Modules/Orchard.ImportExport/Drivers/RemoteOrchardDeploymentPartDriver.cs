using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class RemoteOrchardDeploymentPartDriver : ContentPartDriver<RemoteOrchardDeploymentPart> {

        //GET
        protected override DriverResult Editor(RemoteOrchardDeploymentPart part, dynamic shapeHelper) {
            return ContentShape("Parts_RemoteOrchardDeployment_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Deployment.RemoteOrchardDeployment",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            RemoteOrchardDeploymentPart part, IUpdateModel updater, dynamic shapeHelper) {
            var previousPassword = part.PrivateApiKey;
            updater.TryUpdateModel(part, Prefix, null, null);

            // restore password if the input is empty, meaning it has not been reset
            if (string.IsNullOrEmpty(part.PrivateApiKey)) {
                part.PrivateApiKey = previousPassword;
            }

            return Editor(part, shapeHelper);
        }
    }
}
