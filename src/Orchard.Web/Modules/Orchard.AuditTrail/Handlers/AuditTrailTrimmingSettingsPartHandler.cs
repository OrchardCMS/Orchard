using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Orchard.AuditTrail.Handlers {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingSettingsPartHandler : ContentHandler {
        
        public AuditTrailTrimmingSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<AuditTrailTrimmingSettingsPart>("Site"));
            OnGetContentItemMetadata<AuditTrailSettingsPart>(GetMetadata);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void GetMetadata(GetContentItemMetadataContext context, AuditTrailSettingsPart part) {
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail")));
        }
    }
}