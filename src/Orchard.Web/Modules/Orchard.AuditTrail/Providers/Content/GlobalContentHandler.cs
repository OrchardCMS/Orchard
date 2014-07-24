using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;

namespace Orchard.AuditTrail.Providers.Content {
    public class GlobalContentHandler : ContentHandler {
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentManager _contentManager;
        private XElement _previousVersionXml;
        private readonly IDiffGramAnalyzer _analyzer;

        public GlobalContentHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca, IContentManager contentManager, IDiffGramAnalyzer analyzer) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
            _contentManager = contentManager;
            _analyzer = analyzer;
        }

        protected override void Created(CreateContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Created, context.ContentItem);
        }

        protected override void Updating(UpdateContentContext context) {
            var contentItem = context.ContentItem;
            _previousVersionXml = _contentManager.Export(contentItem);
        }

        protected override void Updated(UpdateContentContext context) {
            var contentItem = context.ContentItem;
            var newVersionXml = _contentManager.Export(contentItem);
            var diffGram = _analyzer.GenerateDiffGram(_previousVersionXml, newVersionXml);

            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Saved, context.ContentItem, diffGram: diffGram, previousVersionXml: _previousVersionXml);
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

        private void RecordAuditTrailEvent(string eventName, IContent content, ContentItemVersionRecord previousContentItemVersion = null, XElement diffGram = null, XElement previousVersionXml = null) {
            var blackList = new[] {"Site"};

            if (blackList.Contains(content.ContentItem.ContentType))
                return;

            var title = _contentManager.GetItemMetadata(content).DisplayText;

            var properties = new Dictionary<string, object> {
                {"Content", content}
            };

            var eventData = new Dictionary<string, object> {
                {"ContentId", content.Id},
                {"ContentIdentity", _contentManager.GetItemMetadata(content).Identity.ToString()},
                {"ContentType", content.ContentItem.ContentType},
                {"VersionId", content.ContentItem.VersionRecord.Id},
                {"VersionNumber", content.ContentItem.VersionRecord.Number},
                {"Published", content.ContentItem.VersionRecord.Published},
            };

            if (previousContentItemVersion != null) {
                eventData["PreviousVersionId"] = previousContentItemVersion.Id;
                eventData["PreviousVersionNumber"] = previousContentItemVersion.Number;
            }

            if (diffGram != null && previousVersionXml != null) {
                eventData["PreviousVersionXml"] = previousVersionXml.ToString(SaveOptions.DisableFormatting);
                eventData["DiffGram"] = diffGram.ToString(SaveOptions.DisableFormatting);
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