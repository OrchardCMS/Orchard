using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;

namespace Orchard.AuditTrail.Providers.Content {
    public class ContentAuditTrailEventShapes : IShapeTableProvider {
        private readonly Work<IContentManager> _contentManager;
        private readonly IDiffGramAnalyzer _analyzer;

        public ContentAuditTrailEventShapes(Work<IContentManager> contentManager, IDiffGramAnalyzer analyzer) {
            _contentManager = contentManager;
            _analyzer = analyzer;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("AuditTrailEvent").OnDisplaying(context => {
                var record = (AuditTrailEventRecord)context.Shape.Record;

                if (record.Category != "Content" || context.ShapeMetadata.DisplayType != "Detail")
                    return;

                var eventData = (IDictionary<string, object>)context.Shape.EventData;
                var contentItemId = eventData.Get<int>("ContentId");
                var previousContentItemVersionId = eventData.Get<int>("PreviousVersionId");
                var contentItem = _contentManager.Value.Get(contentItemId, VersionOptions.Latest);
                var previousVersion = previousContentItemVersionId > 0 ? _contentManager.Value.Get(contentItemId, VersionOptions.VersionRecord(previousContentItemVersionId)) : default(ContentItem);

                if (previousVersion != null) {
                    var previousVersionXml = _contentManager.Value.Export(previousVersion);
                    var currentVersionXml = _contentManager.Value.Export(contentItem);
                    var diffGram = _analyzer.GenerateDiffGram(previousVersionXml, currentVersionXml);
                    var diffNodes = _analyzer.Analyze(previousVersionXml, diffGram).ToArray();
                    context.Shape.DiffNodes = diffNodes;
                }

                context.Shape.ContentItem = contentItem;
                context.Shape.PreviousVersion = previousVersion;
            });
        }
    }
}