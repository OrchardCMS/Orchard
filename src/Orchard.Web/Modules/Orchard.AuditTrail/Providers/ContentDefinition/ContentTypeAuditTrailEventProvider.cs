using System;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentTypeDefinition")]
    public class ContentTypeAuditTrailEventProvider : AuditTrailEventProviderBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypeAuditTrailEventProvider(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string PartAdded = "PartAdded";
        public const string PartRemoved = "PartRemoved";
        public const string TypeSettingsUpdated = "TypeSettingsUpdated";
        public const string PartSettingsUpdated = "PartSettingsUpdated";

        public override void Describe(DescribeContext context) {
            context.For("ContentType", T("Content Type"))
                .Event(this, Created, T("Created"), T("Content Type was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("Content Type was removed."), enableByDefault: true)
                .Event(this, PartAdded, T("Part added"), T("Content Part was added."), enableByDefault: true)
                .Event(this, PartRemoved, T("Part removed"), T("Content Part was removed."), enableByDefault: true)
                .Event(this, TypeSettingsUpdated, T("Type Settings updated"), T("Content Type settings were updated."), enableByDefault: true)
                .Event(this, PartSettingsUpdated, T("Part Settings updated"), T("Content Part settings were updated."), enableByDefault: true);

            context.QueryFilter(QueryFilter);
            context.DisplayFilter(DisplayFilter);
        }

        private void QueryFilter(QueryFilterContext context) {
            var contentType = context.Filters.Get("contenttype");

            if(String.IsNullOrWhiteSpace(contentType))
                return;

            context.Query = context.Query.Where(x => x.EventFilterKey == "contenttype" && x.EventFilterData == contentType);
        }

        private void DisplayFilter(DisplayFilterContext context) {
            var filterDisplay = context.ShapeFactory.AuditTrailFilter__ContentType(
                ContentType: context.Filters.Get("contenttype"),
                ContentTypes: _contentDefinitionManager.ListTypeDefinitions().OrderBy(x => x.DisplayName).ToArray());

            context.FilterLayout.TripleFirst.Add(filterDisplay);
        }
    }
}