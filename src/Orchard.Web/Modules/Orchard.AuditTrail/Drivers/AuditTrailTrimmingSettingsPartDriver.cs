using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingSettingsPartDriver : ContentPartDriver<AuditTrailTrimmingSettingsPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IDateServices _dateServices;
        private readonly IDateTimeFormatProvider _dateTimeLocalization;

        public AuditTrailTrimmingSettingsPartDriver(IAuthorizer authorizer, IDateServices dateServices, IDateTimeFormatProvider dateTimeLocalization) {
            _authorizer = authorizer;
            _dateServices = dateServices;
            _dateTimeLocalization = dateTimeLocalization;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(AuditTrailTrimmingSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AuditTrailTrimmingSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizer.Authorize(Permissions.ManageAuditTrailSettings))
                return null;

            return ContentShape("Parts_AuditTrailTrimmingSettings_Edit", () => {
                var viewModel = new AuditTrailTrimmingSettingsViewModel {
                    RetentionPeriod = part.RetentionPeriod,
                    MinimumRunInterval = part.MinimumRunInterval,
                    LastRunDateString = _dateServices.ConvertToLocalString(part.LastRunUtc, _dateTimeLocalization.ShortDateTimeFormat, T("Never").Text)
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.RetentionPeriod = viewModel.RetentionPeriod;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailTrimmingSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}