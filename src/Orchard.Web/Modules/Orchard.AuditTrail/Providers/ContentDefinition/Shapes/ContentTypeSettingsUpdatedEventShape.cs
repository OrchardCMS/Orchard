using System.Collections.Generic;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Shapes;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition.Shapes {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class ContentTypeSettingsUpdatedEventShape : AuditTrailEventShapeAlteration<ContentTypeAuditTrailEventProvider> {
        private readonly ISettingsFormatter _settingsFormatter;
        public ContentTypeSettingsUpdatedEventShape(ISettingsFormatter settingsFormatter) {
            _settingsFormatter = settingsFormatter;
        }

        protected override string EventName {
            get { return ContentTypeAuditTrailEventProvider.TypeSettingsUpdated; }
        }

        protected override void OnAlterShape(ShapeDisplayingContext context) {
            var eventData = (IDictionary<string, object>)context.Shape.EventData;
            var oldSettings = _settingsFormatter.Map(XmlHelper.Parse((string)eventData["OldSettings"]));
            var newSettings = _settingsFormatter.Map(XmlHelper.Parse((string)eventData["NewSettings"]));
            var diff = oldSettings.GetDiff(newSettings);

            context.Shape.OldSettings = oldSettings;
            context.Shape.NewSettings = newSettings;
            context.Shape.Diff = diff;
        }
    }
}