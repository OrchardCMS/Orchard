using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.ViewModels;
using Orchard.Localization;

namespace Orchard.ImportExport.Drivers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentUserPartDriver : ContentPartDriver<DeploymentUserPart> {
        public DeploymentUserPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        //GET
        protected override DriverResult Editor(DeploymentUserPart part, dynamic shapeHelper) {

            var model = new EditDeploymentUserViewModel {
                PrivateApiKey = part.PrivateApiKey,
                EnableApiAccess = !string.IsNullOrEmpty(part.PrivateApiKey),
            };

            return ContentShape("Parts_DeploymentUser_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Deployment.DeploymentUser",
                    Model: model,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(
            DeploymentUserPart part, IUpdateModel updater, dynamic shapeHelper) {

            var model = new EditDeploymentUserViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);

            if (model.EnableApiAccess) {
                if (string.IsNullOrEmpty(model.PrivateApiKey) && string.IsNullOrEmpty(part.PrivateApiKey)) {
                    updater.AddModelError("PrivateApiKey", T("If API access is enabled, the private API key is required."));
                }
                else if (!string.IsNullOrEmpty(model.PrivateApiKey) && model.PrivateApiKey.Length < 20) {
                    updater.AddModelError("PrivateApiKey", T("API key must be a minimum of 20 characters."));
                }
                else if (!string.IsNullOrEmpty(model.PrivateApiKey)) {
                    part.PrivateApiKey = model.PrivateApiKey;
                }
                //Otherwise key has not been updated.
            }
            else {
                part.PrivateApiKey = null;
            }

            return Editor(part, shapeHelper);
        }
    }
}
