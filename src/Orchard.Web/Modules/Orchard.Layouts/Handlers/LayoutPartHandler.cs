using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Layouts.Models;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Handlers {
    public class LayoutPartHandler : ContentHandler {
        private readonly ILayoutManager _layoutManager;
        private readonly IContentManager _contentManager;
        private readonly IContentPartDisplay _contentPartDisplay;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ILayoutSerializer _serializer;

        public LayoutPartHandler(
            IRepository<LayoutPartRecord> repository, 
            ILayoutManager layoutManager, 
            IContentManager contentManager, 
            IContentPartDisplay contentPartDisplay, 
            IShapeDisplay shapeDisplay, 
            ILayoutSerializer serializer) {

            _layoutManager = layoutManager;
            _contentManager = contentManager;
            _contentPartDisplay = contentPartDisplay;
            _shapeDisplay = shapeDisplay;
            _serializer = serializer;

            Filters.Add(StorageFilter.For(repository));
            OnPublished<LayoutPart>(UpdateTemplateClients);
            OnIndexing<LayoutPart>(IndexLayout);
        }

        private void IndexLayout(IndexContentContext context, LayoutPart part) {
            var layoutShape = _contentPartDisplay.BuildDisplay(part);
            var layoutHtml = _shapeDisplay.Display(layoutShape);

            context.DocumentIndex
                .Add("body", layoutHtml).RemoveTags().Analyze()
                .Add("format", "html").Store();
        }

        private void UpdateTemplateClients(PublishContentContext context, LayoutPart part) {
            UpdateTemplateClients(part);
        }

        /// <summary>
        /// Recursively updates all layouts that use the specified layout as its template.
        /// </summary>
        private void UpdateTemplateClients(LayoutPart part) {
            if (!part.IsTemplate)
                return;

            // When a template has changed, we need to update all layouts that use this template.
            // If the layout is a draft, we will update that one.
            // If the layout is published, we will require a new draft, perform the update, and then publish.
            var templateClients = _layoutManager.GetTemplateClients(part.Id, VersionOptions.Latest);

            foreach (var layout in templateClients) {
                var isPublished = layout.ContentItem.VersionRecord.Published;
                var draft = isPublished ? _contentManager.Get<LayoutPart>(layout.Id, VersionOptions.DraftRequired) : layout;
                var updatedLayout = _layoutManager.ApplyTemplate(layout, part);

                draft.LayoutData = _serializer.Serialize(updatedLayout);

                if (isPublished) {
                    // We don't have to recurse here, since invoking Publish on a Layout will cause this handler to execute again.
                    _contentManager.Publish(draft.ContentItem);
                }
                else if (layout.IsTemplate) {
                    UpdateTemplateClients(draft);
                }
            }
        }
    }
}