using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class MediaItemElementDriver : ElementDriver<MediaItem> {
        private readonly IContentManager _contentManager;

        public MediaItemElementDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(MediaItem element, ElementEditorContext context) {
            
            var viewModel = new ContentItemEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.MediaItem", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.MediaItemIds = ContentItem.Deserialize(viewModel.ContentItemIds);
                element.DisplayType = viewModel.DisplayType;
            }

            var contentItemIds = element.MediaItemIds;
            var displayType = element.DisplayType;
            var layoutPart = context.Content;
            var layoutId = layoutPart != null ? layoutPart.Id : 0;

            viewModel.ContentItems = GetContentItems(RemoveCurrentContentItemId(contentItemIds, layoutId)).ToArray();
            viewModel.DisplayType = displayType;

            return Editor(context, editor);
        }

        protected override void OnDisplaying(MediaItem element, ElementDisplayingContext context) {
            var contentItemIds = RemoveCurrentContentItemId(element.MediaItemIds, context.Content.Id);
            var displayType = context.DisplayType != "Design" ? element.DisplayType : "Thumbnail";
            var contentItems = GetContentItems(contentItemIds).ToArray();
            var contentItemShapes = contentItems.Select(x => _contentManager.BuildDisplay(x, displayType)).ToArray();

            context.ElementShape.ContentItems = contentItemShapes;
        }

        protected override void OnExporting(MediaItem element, ExportElementContext context) {
            var contentItems = GetContentItems(element.MediaItemIds).ToArray();

            if (!contentItems.Any())
                return;

            var identities = contentItems.Select(x => _contentManager.GetItemMetadata(x).Identity.ToString()).ToArray();
            context.ExportableData["ContentItems"] = String.Join(",", identities);
        }

        protected override void OnImporting(MediaItem element, ImportElementContext context) {
            var contentItemIdentities = context.ExportableData.Get("ContentItems");

            if (String.IsNullOrWhiteSpace(contentItemIdentities))
                return;

            var identities = contentItemIdentities.Split(',');
            var contentItems = identities.Select(x => context.Session.GetItemFromSession(x)).Where(x => x != null);

            element.MediaItemIds = contentItems.Select(x => x.Id);
        }

        protected IEnumerable<ContentManagement.ContentItem> GetContentItems(IEnumerable<int> ids) {
            return _contentManager.GetMany<IContent>(ids, VersionOptions.Published, QueryHints.Empty).Select(x => x.ContentItem);
        }

        // The user can't pick the content that will host the selected content to prevent an infinite loop / stack overflow.
        protected IEnumerable<int> RemoveCurrentContentItemId(IEnumerable<int> ids, int currentId) {
            return ids.Where(x => x != currentId);
        }
    }
}