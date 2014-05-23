using System.Collections.Generic;
using Orchard.AuditTrail.Providers;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.AuditTrail.Handlers {
    public class GlobalContentHandler : ContentHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentManager _contentManager;

        public GlobalContentHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca, IContentManager contentManager) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
            _contentManager = contentManager;
        }

        protected override void Created(CreateContentContext context) {
            RecordAuditTrail(ContentAuditTrailEventProvider.Created, context.ContentItem);
        }

        protected override void Updated(UpdateContentContext context) {
            RecordAuditTrail(ContentAuditTrailEventProvider.Saved, context.ContentItem);
        }

        protected override void Published(PublishContentContext context) {
            RecordAuditTrail(ContentAuditTrailEventProvider.Published, context.ContentItem);
        }

        protected override void Unpublished(PublishContentContext context) {
            RecordAuditTrail(ContentAuditTrailEventProvider.Unpublished, context.ContentItem);
        }

        protected override void Removed(RemoveContentContext context) {
            RecordAuditTrail(ContentAuditTrailEventProvider.Removed, context.ContentItem);
        }

        private void RecordAuditTrail(string eventName, IContent content) {
            var title = _contentManager.GetItemMetadata(content).DisplayText;
            _auditTrailManager.Record(eventName, _wca.GetContext().CurrentUser, content, new Dictionary<string, object> {
                {"Title", title}
            });
        }
    }
}