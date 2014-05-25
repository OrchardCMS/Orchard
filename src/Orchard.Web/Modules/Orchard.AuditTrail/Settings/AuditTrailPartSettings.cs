using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.AuditTrail.Settings {
    public class AuditTrailPartSettings {
        public bool ShowAuditTrailLink { get; set; }
        public bool ShowAuditTrail { get; set; }

        public void Build(ContentTypePartDefinitionBuilder builder) {
            builder.WithSetting("AuditTrailPartSettings.ShowAuditTrailLink", ShowAuditTrailLink.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("AuditTrailPartSettings.ShowAuditTrail", ShowAuditTrail.ToString(CultureInfo.InvariantCulture));
        }
    }
}