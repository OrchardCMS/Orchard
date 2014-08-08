using System;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;

namespace Orchard.AuditTrail.Providers.ContentDefinition {
    [OrchardFeature("Orchard.AuditTrail.ContentDefinition")]
    public class ContentTypeAuditTrailEventProvider : AuditTrailEventProviderBase {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentTypeAuditTrailEventProvider(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public const string Created = "Created";
        public const string Removed = "Removed";
        public const string Imported = "Imported";
        public const string PartAdded = "PartAdded";
        public const string PartRemoved = "PartRemoved";
        public const string TypeDisplayNameUpdated = "TypeDisplayNameUpdated";
        public const string TypeSettingsUpdated = "TypeSettingsUpdated";
        public const string PartSettingsUpdated = "PartSettingsUpdated";
        public const string FieldSettingsUpdated = "FieldSettingsUpdated";

        public override void Describe(DescribeContext context) {
            context.For("ContentType", T("Content Type"))
                .Event(this, Created, T("Created"), T("A content type was created."), enableByDefault: true)
                .Event(this, Removed, T("Removed"), T("A content type was removed."), enableByDefault: true)
                .Event(this, Imported, T("Imported"), T("A content type was imported."), enableByDefault: true)
                .Event(this, PartAdded, T("Part added"), T("A content part was added to a content type."), enableByDefault: true)
                .Event(this, PartRemoved, T("Part removed"), T("A content part was removed from a content type."), enableByDefault: true)
                .Event(this, TypeDisplayNameUpdated, T("Type display name updated"), T("The display name of a content type was updated."), enableByDefault: true)
                .Event(this, TypeSettingsUpdated, T("Type settings updated"), T("The settings of a content type were updated."), enableByDefault: true)
                .Event(this, PartSettingsUpdated, T("Part settings updated"), T("The settings of a content part on a content type were updated."), enableByDefault: true)
                .Event(this, FieldSettingsUpdated, T("Field settings updated"), T("The settings of a content field on a content part on a content type were updated."), enableByDefault: true);

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

            context.FilterDisplay.Add(filterDisplay);
        }
    }
}