using System;
using System.Collections.Generic;
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
                localField._contentItems = new Lazy<IEnumerable<MediaPart>>(() => _contentManager.GetMany<MediaPart>(localField.Ids, VersionOptions.Published, QueryHints.Empty).ToList());
            }
        }
    }
}