using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class ContentPartAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string Imported = "Imported";
        public const string DescriptionChanged = "DescriptionChanged";
        public const string FieldAdded = "FieldAdded";
        public const string FieldRemoved = "FieldRemoved";
        public const string PartSettingsUpdated = "PartSettingsUpdated";
        public const string FieldSettingsUpdated = "FieldSettingsUpdated";

        public override void Describe(DescribeContext context) {
            context.For("ContentPart", T("Content Parts"))
                .Event(this, Created, T("Created"), T("A content part was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("A content part was removed."), enableByDefault: true)
                .Event(this, Imported, T("Imported"), T("A content part was imported."), enableByDefault: true)
                .Event(this, DescriptionChanged, T("Description changed"), T("A content part description was changed."), enableByDefault: true)
                .Event(this, FieldAdded, T("Field added"), T("A field was added to a content part."), enableByDefault: true)
                .Event(this, FieldRemoved, T("Field removed"), T("A field was removed from a content part."), enableByDefault: true)
                .Event(this, PartSettingsUpdated, T("Part settings updated"), T("The settings of a content part were updated."), enableByDefault: true)
                .Event(this, FieldSettingsUpdated, T("Field settings updated"), T("The settings of a field on a content part were updated."), enableByDefault: true);
        }
    }
}