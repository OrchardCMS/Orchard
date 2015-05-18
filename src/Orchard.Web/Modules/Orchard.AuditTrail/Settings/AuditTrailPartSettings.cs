using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.AuditTrail.Settings {
    public class AuditTrailPartSettings {
        public AuditTrailPartSettings() {
            ShowAuditTrail = true;
            ShowAuditTrailCommentInput = true;
            ShowAuditTrailLink = true;
        }

        public bool ShowAuditTrailLink { get; set; }
        public bool ShowAuditTrail { get; set; }
        public bool ShowAuditTrailCommentInput { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("AuditTrailPartSettings.ShowAuditTrailLink", ShowAuditTrailLink.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AuditTrailPartSettings.ShowAuditTrail", ShowAuditTrail.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AuditTrailPartSettings.ShowAuditTrailCommentInput", ShowAuditTrailCommentInput.ToString(CultureInfo.InvariantCulture));
        }
    }
}