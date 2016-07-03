using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Templates.Models;
using System.Linq;

namespace Orchard.Templates.Drivers {
    public class TitlePartDriver : ContentPartDriver<TitlePart> {
        private readonly IContentManager _contentManager;

        public Localizer T { get; set; }

        public TitlePartDriver(IContentManager contentManager) {
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(TitlePart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!part.ContentItem.Has<ShapePart>()) {
                return null;
            }

            updater.TryUpdateModel(part, Prefix, null, null);

            // We need to query for the content type names because querying for content parts has no effect on the database side.
            var contentTypesWithShapePart = _contentManager
                .GetContentTypeDefinitions()
                .Where(typeDefinition => typeDefinition.Parts.Any(partDefinition => partDefinition.PartDefinition.Name == "ShapePart"))
                .Select(typeDefinition => typeDefinition.Name);

            // If ShapePart is only dynamically added to this content type or even this content item then we won't find
            // a corresponding content type definition, so using the current content type too.
            contentTypesWithShapePart = contentTypesWithShapePart.Union(new[] { part.ContentItem.ContentType });

            var existingShapeCount = _contentManager
                .Query(VersionOptions.Latest, contentTypesWithShapePart.ToArray())
                .Where<TitlePartRecord>(record => record.Title == part.Title && record.ContentItemRecord.Id != part.ContentItem.Id)
                .Count();

            if (existingShapeCount > 0) {
                updater.AddModelError("ShapeNameAlreadyExists", T("A template with the given name already exists."));
            }

            return null;
        }
    }
}