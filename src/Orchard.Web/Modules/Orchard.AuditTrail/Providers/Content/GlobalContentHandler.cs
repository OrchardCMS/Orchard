using System.Collections.Generic;
using System.Globalization;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;

namespace Orchard.AuditTrail.Providers.Content {
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
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Created, context.ContentItem);
        }

        protected override void Updated(UpdateContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Saved, context.ContentItem);
        }

        protected override void Published(PublishContentContext context) {
            var previousVersion = context.PreviousItemVersionRecord;
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Published, context.ContentItem, previousVersion);
        }

        protected override void Unpublished(PublishContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Unpublished, context.ContentItem);
        }

        protected override void Removed(RemoveContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Removed, context.ContentItem);
        }

        private void RecordAuditTrailEvent(string eventName, IContent content, ContentItemVersionRecord previousContentItemVersion = null) {
            var title = _contentManager.GetItemMetadata(content).DisplayText;

            var properties = new Dictionary<string, object> {
                {"Content", content}
            };

            var eventData = new Dictionary<string, object> {
                {"ContentItemId", content.Id},
                {"ContentItemVersionId", content.ContentItem.VersionRecord.Id},
                {"ContentItemVersionNumber", content.ContentItem.VersionRecord.Number},
                {"Title", title}
            };

            if (previousContentItemVersion != null) {
                eventData["PreviousContentItemVersionId"] = previousContentItemVersion.Id;
            }

            _auditTrailManager.CreateRecord<ContentAuditTrailEventProvider>(
                eventName, 
                _wca.GetContext().CurrentUser, 
                properties, 
                eventData, 
                eventFilterKey: "content", 
                eventFilterData: content.Id.ToString(CultureInfo.InvariantCulture));
        }
    }
}