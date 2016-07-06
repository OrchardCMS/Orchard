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
        private bool _contentItemCreated = false;
        private ContentItem _ignoreExportHandlerFor;

        public GlobalContentHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca, IContentManager contentManager, IDiffGramAnalyzer analyzer) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
            _contentManager = contentManager;
            _analyzer = analyzer;
        }

        protected override void Created(CreateContentContext context) {
            // At this point the UpdateEditor hasn't been invoked on the content item yet,
            // so we don't have access to all of the information we might need (such as Title).
            // We set a flag which we will check in the Updated method (which is invoked when UpdateEditor is invoked).
            _contentItemCreated = true;
        }

        protected override void Updating(UpdateContentContext context) {
            var contentItem = context.ContentItem;

            if (contentItem.IsNew())
                return;

                _ignoreExportHandlerFor = contentItem;
            _previousVersionXml = _contentItemCreated 
                ? default(XElement) // No need to do a diff on a newly created content item.
                : _contentManager.Export(contentItem);
            _ignoreExportHandlerFor = null;
        }

        protected override void Updated(UpdateContentContext context) {
            var contentItem = context.ContentItem;

            if (contentItem.IsNew())
                return;

            if (_contentItemCreated) {
                RecordAuditTrailEvent(ContentAuditTrailEventProvider.Created, context.ContentItem);
            }
            else {
                _ignoreExportHandlerFor = contentItem;
                var newVersionXml = _contentManager.Export(contentItem);
                _ignoreExportHandlerFor = null;

                var diffGram = _analyzer.GenerateDiffGram(_previousVersionXml, newVersionXml);
                RecordAuditTrailEvent(ContentAuditTrailEventProvider.Saved, context.ContentItem, diffGram: diffGram, previousVersionXml: _previousVersionXml);    
            }
        }

        protected override void Restoring(RestoreContentContext context) {
            _ignoreExportHandlerFor = context.ContentItem;
            _previousVersionXml = _contentManager.Export(context.ContentItem);
            _ignoreExportHandlerFor = null;
        }

        protected override void Restored(RestoreContentContext context) {
            var contentItem = context.ContentItem;
           
            _ignoreExportHandlerFor = contentItem;
            var newVersionXml = _contentManager.Export(contentItem);
            _ignoreExportHandlerFor = null;

            var diffGram = _analyzer.GenerateDiffGram(_previousVersionXml, newVersionXml);
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Restored, context.ContentItem, diffGram: diffGram, previousVersionXml: _previousVersionXml);
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

        protected override void Destroyed(DestroyContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Destroyed, context.ContentItem);
        }

        protected override void Imported(ImportContentContext context) {
            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Imported, context.ContentItem);
        }

        protected override void Exported(ExportContentContext context) {
            if (context.ContentItem == _ignoreExportHandlerFor)
                return;

            RecordAuditTrailEvent(ContentAuditTrailEventProvider.Exported, context.ContentItem);
        }

        private void RecordAuditTrailEvent(string eventName, IContent content, ContentItemVersionRecord previousContentItemVersion = null, XElement diffGram = null, XElement previousVersionXml = null) {
            var blackList = new[] {"Site"};

            if (blackList.Contains(content.ContentItem.ContentType))
                return;

            var properties = new Dictionary<string, object> {
                {"Content", content}
            };

            var metaData = _contentManager.GetItemMetadata(content);
            var eventData = new Dictionary<string, object> {
                {"ContentId", content.Id},
                {"ContentIdentity", metaData.Identity.ToString()},
                {"ContentType", content.ContentItem.ContentType},
                {"VersionId", content.ContentItem.Version},
                {"VersionNumber", content.ContentItem.VersionRecord != null ? content.ContentItem.VersionRecord.Number : 0},
                {"Published", content.ContentItem.VersionRecord != null && content.ContentItem.VersionRecord.Published},
                {"Title", metaData.DisplayText}
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