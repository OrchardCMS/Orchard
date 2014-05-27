using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    public class ContentTypeAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string PartAdded = "PartAdded";
        public const string PartRemoved = "PartRemoved";
        public const string TypeSettingsUpdated = "TypeSettingsUpdated";
        public const string PartSettingsUpdated = "PartSettingsUpdated";

        public override void Describe(DescribeContext context) {
            context.For("ContentType", T("Content Type"))
                .Event(this, Created, T("Created"), T("Content Type was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("Content Type was removed."), enableByDefault: true)
                .Event(this, PartAdded, T("Part added"), T("Content Part was added."), enableByDefault: true)
                .Event(this, PartRemoved, T("Part removed"), T("Content Part was removed."), enableByDefault: true)
                .Event(this, TypeSettingsUpdated, T("Type Settings updated"), T("Content Type settings were updated."), enableByDefault: true)
                .Event(this, PartSettingsUpdated, T("Part Settings updated"), T("Content Part settings were updated."), enableByDefault: true);
        }
    }
}