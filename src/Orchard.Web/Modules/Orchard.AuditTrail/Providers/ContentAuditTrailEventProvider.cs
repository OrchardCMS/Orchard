using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;

namespace Orchard.AuditTrail.Providers {
    public class ContentAuditTrailEventProvider : AuditTrailEventProviderBase {
        public const string Created = "Created";
        public const string Saved = "Saved";
        public const string Published = "Published";
        public const string Unpublished = "Unpublished";
        public const string Removed = "Removed";

        public override void Describe(DescribeContext context) {
            context.For("Content", T("Content"))
                .Event(Created, T("Created"), T("Content was created."), enableByDefault: true)
                .Event(Saved, T("Saved"), T("Content was saved."), enableByDefault: true)
                .Event(Published, T("Published"), T("Content was published."), enableByDefault: true)
                .Event(Unpublished, T("Unpublished"), T("Content was unpublished."), enableByDefault: true)
                .Event(Removed, T("Removed"), T("Content was deleted."), enableByDefault: true);
        }
    }
}