using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class ContentItemElementDriver : ElementDriver<ContentItem> {
        private readonly IContentManager _contentManager;

        public ContentItemElementDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(ContentItem element, ElementEditorContext context) {
            var layoutPart = context.Content;
            var viewModel = new ContentItemEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.ContentItem", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.ContentItemIds = ContentItem.Deserialize(viewModel.ContentItemIds);
                element.DisplayType = viewModel.DisplayType;
            }

            var contentItemIds = element.ContentItemIds;
            var displayType = element.DisplayType;
            var layoutContentId = layoutPart != null ? layoutPart.Id : 0;

            viewModel.ContentItems = GetContentItems(RemoveCurrentContentItemId(contentItemIds, layoutContentId)).ToArray();
            viewModel.DisplayType = displayType;

            return Editor(context, editor);
        }

        protected override void OnDisplaying(ContentItem element, ElementDisplayContext context) {
            var contentItemIds = context.Content != null ? RemoveCurrentContentItemId(element.ContentItemIds, context.Content.Id) : element.ContentItemIds;
            var displayType = element.DisplayType;
            var contentItems = GetContentItems(contentItemIds).ToArray();
            var contentItemShapes = contentItems.Select(x => _contentManager.BuildDisplay(x, displayType)).ToArray();

            context.ElementShape.ContentItems = contentItemShapes;
        }

        protected override void OnExporting(ContentItem element, ExportElementContext context) {
            var contentItems = GetContentItems(element.ContentItemIds).ToArray();

            if (!contentItems.Any())
                return;

            var identities = contentItems.Select(x => _contentManager.GetItemMetadata(x).Identity.ToString()).ToArray();
            context.ExportableData["ContentItems"] = String.Join(",", identities);
        }

        protected override void OnImporting(ContentItem element, ImportElementContext context) {
            var contentItemIdentities = context.ExportableData.Get("ContentItems");

            if (String.IsNullOrWhiteSpace(contentItemIdentities))
                return;

            var identities = contentItemIdentities.Split(',');
            var contentItems = identities.Select(x => context.Session.GetItemFromSession(x)).Where(x => x != null);

            element.ContentItemIds = contentItems.Select(x => x.Id);
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