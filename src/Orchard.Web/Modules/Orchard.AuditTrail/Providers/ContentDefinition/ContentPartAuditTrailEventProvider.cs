using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentTypeDefinition")]
    public class ContentPartAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string DescriptionChanged = "DescriptionChanged";
        public const string FieldAdded = "FieldAdded";
        public const string FieldRemoved = "FieldRemoved";
        public const string PartSettingsUpdated = "PartSettingsUpdated";
        public const string FieldSettingsUpdated = "FieldSettingsUpdated";

        public override void Describe(DescribeContext context) {
            context.For("ContentPart", T("Content Part"))
                .Event(this, Created, T("Created"), T("Content Type was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("Content Type was removed."), enableByDefault: true)
                .Event(this, DescriptionChanged, T("Description changed"), T("Content Part description was changed."), enableByDefault: true)
                .Event(this, FieldAdded, T("Field added"), T("Content Field was added."), enableByDefault: true)
                .Event(this, FieldRemoved, T("Field removed"), T("Content Field was removed."), enableByDefault: true)
                .Event(this, PartSettingsUpdated, T("Part settings updated"), T("Content Part settings were updated."), enableByDefault: true)
                .Event(this, FieldSettingsUpdated, T("Field settings updated"), T("Content Field settings were updated."), enableByDefault: true);
        }
    }
}