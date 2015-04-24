using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Shapes;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition.Shapes {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class ContentPartImportedEventShape : AuditTrailEventShapeAlteration<ContentPartAuditTrailEventProvider> {
        private readonly IDiffGramAnalyzer _analyzer;

        public ContentPartImportedEventShape(IDiffGramAnalyzer analyzer) {
            _analyzer = analyzer;
        }

        protected override string EventName {
            get { return ContentPartAuditTrailEventProvider.Imported; }
        }

        protected override void OnAlterShape(ShapeDisplayingContext context) {
            var eventData = (IDictionary<string, object>)context.Shape.EventData;
            var previousDefinitionXml = eventData.GetXml("PreviousDefinition");
            var newDefinitionXml = eventData.GetXml("NewDefinition");
            var diffGram = eventData.GetXml("DiffGram");

            context.Shape.NewDefinitionXml = newDefinitionXml;

            if (diffGram != null) {
                var diffNodes = _analyzer.Analyze(previousDefinitionXml, diffGram).ToArray();
                context.Shape.DiffNodes = diffNodes;
            }
        }
    }
}