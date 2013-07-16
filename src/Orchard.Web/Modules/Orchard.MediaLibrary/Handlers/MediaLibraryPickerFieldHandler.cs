using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Handlers {
    public class MediaLibraryPickerFieldHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MediaLibraryPickerFieldHandler(
            IContentManager contentManager, 
            IContentDefinitionManager contentDefinitionManager) {
            
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override void Loading(LoadContentContext context) {
            base.Loading(context);

            var fields = context.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(MediaLibraryPickerField).Name)).Cast<MediaLibraryPickerField>();
            
            // define lazy initializer for ContentPickerField.ContentItems
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }

            foreach (var field in fields) {
                var localField = field;
                field._contentItems.Loader(x => _contentManager.GetMany<MediaPart>(localField.Ids, VersionOptions.Published, QueryHints.Empty));
            }
        }
    }
}