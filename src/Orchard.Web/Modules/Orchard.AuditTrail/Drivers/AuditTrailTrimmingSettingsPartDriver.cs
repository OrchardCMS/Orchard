using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingSettingsPartDriver : ContentPartDriver<AuditTrailTrimmingSettingsPart> {
        private readonly IAuthorizer _authorizer;

        public AuditTrailTrimmingSettingsPartDriver(IAuthorizer authorizer) {
            _authorizer = authorizer;
        }

        protected override DriverResult Editor(AuditTrailTrimmingSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailTrimmingSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAuditTrailSettings))
                return null;

            return ContentShape("Parts_AuditTrailTrimmingSettings_Edit", () => {
                var viewModel = new AuditTrailTrimmingSettingsViewModel {
                    Threshold = part.Threshold,
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.Threshold = viewModel.Threshold;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailTrimmingSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}