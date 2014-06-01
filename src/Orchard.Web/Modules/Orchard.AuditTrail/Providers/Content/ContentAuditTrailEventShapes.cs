using System.Collections.Generic;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;

namespace Orchard.AuditTrail.Providers.Content {
    public class ContentAuditTrailEventShapes : IShapeTableProvider {
        private readonly Work<IContentManager> _contentManager;
        public ContentAuditTrailEventShapes(Work<IContentManager> contentManager) {
            _contentManager = contentManager;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("AuditTrailEvent").OnDisplaying(context => {
                var record = (AuditTrailEventRecord)context.Shape.Record;

                if (record.Category != "Content" || context.ShapeMetadata.DisplayType != "Detail")
                    return;

                var eventData = (IDictionary<string, object>)context.Shape.EventData;
                var contentItemId = eventData.Get<int>("ContentItemId");
                var previousContentItemVersionId = eventData.Get<int>("PreviousContentItemVersionId");
                var contentItem = _contentManager.Value.Get(contentItemId);
                var previousVersion = previousContentItemVersionId > 0 ? _contentManager.Value.Get(contentItemId, VersionOptions.VersionRecord(previousContentItemVersionId)) : default(ContentItem);

                context.Shape.ContentItem = contentItem;
                context.Shape.PreviousVersion = previousVersion;
            });
        }
    }
}