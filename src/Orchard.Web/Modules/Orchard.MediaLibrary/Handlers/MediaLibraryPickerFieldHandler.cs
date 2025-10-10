using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaLibraryPickerFieldHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MediaLibraryPickerFieldHandler(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override void Loaded(LoadContentContext context) {
            base.Loaded(context);
            InitilizeLoader(context.ContentItem);
        }

        private void InitilizeLoader(ContentItem contentItem) {
            var fields = contentItem.Parts.SelectMany(x => x.Fields.OfType<MediaLibraryPickerField>());

            // define lazy initializer for MediaLibraryPickerField.MediaParts
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }

            foreach (var field in fields) {
                var localField = field;
                // Using context content item's ContentManager instead of injected one to avoid lifetime scope exceptions in case of LazyFields.
                localField._contentItems.Loader(() =>
                    contentItem.ContentManager.GetMany<MediaPart>(localField.Ids, VersionOptions.Published, QueryHints.Empty).ToList());
            }
        }
    }
}