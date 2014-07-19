using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
                var previousVersionXml = GetXml(eventData, "PreviousVersionXml");
                var diffGram = GetXml(eventData, "DiffGram");
                var contentItem = _contentManager.Value.Get(contentItemId, VersionOptions.Latest);
                var previousVersion = previousContentItemVersionId > 0 ? _contentManager.Value.Get(contentItemId, VersionOptions.VersionRecord(previousContentItemVersionId)) : default(ContentItem);

                if (diffGram != null) {
                    var diffNodes = _analyzer.Analyze(previousVersionXml, diffGram).ToArray();
                    context.Shape.DiffNodes = diffNodes;
                }

                context.Shape.ContentItem = contentItem;
                context.Shape.PreviousVersion = previousVersion;
            });
        }

        private static XElement GetXml(IDictionary<string, object> eventData, string key) {
            var data = eventData.Get<string>(key);

            if (String.IsNullOrWhiteSpace(data))
                return null;

            try {
                return XElement.Parse(data);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}