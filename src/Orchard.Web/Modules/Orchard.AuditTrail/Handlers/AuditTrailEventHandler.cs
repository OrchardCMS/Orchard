using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Handlers {
    public class AuditTrailEventHandler : IAuditTrailEventHandler {
        public void Create(AuditTrailCreateContext context) {
            var content = (IContent)context.Properties["Content"];
            var auditTrailPart = content != null ? content.As<AuditTrailCommentPart>() : default(AuditTrailCommentPart);

            if (auditTrailPart == null)
                return;

            context.Comment = auditTrailPart.Comment;
            auditTrailPart.Comment = null; // Reset the comment.
        }
    }
}