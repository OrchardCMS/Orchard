using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Security;

namespace Orchard.AuditTrail.Drivers {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingSettingsPartDriver : ContentPartDriver<AuditTrailTrimmingSettingsPart> {
        private readonly IAuthorizer _authorizer;
        private readonly IDateLocalizationServices _dateLocalizationServices;
        private readonly IDateTimeFormatProvider _dateTimeLocalization;

        public AuditTrailTrimmingSettingsPartDriver(IAuthorizer authorizer, IDateLocalizationServices dateLocalizationServices, IDateTimeFormatProvider dateTimeLocalization) {
            _authorizer = authorizer;
            _dateLocalizationServices = dateLocalizationServices;
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
                    LastRunDateString = _dateLocalizationServices.ConvertToLocalizedString(part.LastRunUtc, _dateTimeLocalization.ShortDateTimeFormat, new DateLocalizationOptions() { NullText = T("Never").Text })
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.RetentionPeriod = viewModel.RetentionPeriod;
                        part.MinimumRunInterval = viewModel.MinimumRunInterval;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts.AuditTrailTrimmingSettings", Model: viewModel, Prefix: Prefix);
            }).OnGroup("Audit Trail");
        }
    }
}