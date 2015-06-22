using System;
using System.Collections.Generic;
using System.Linq;
using IDeliverable.Slides.Helpers;
using IDeliverable.Slides.Services;
using IDeliverable.Slides.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Services;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Helpers;
using Orchard.Localization;

namespace IDeliverable.Slides.SlidesProviders
{
    [OrchardFeature("IDeliverable.Slides.Lists")]
    public class ListSlidesProvider : SlidesProvider
    {
        private readonly IContentManager _contentManager;
        private readonly IContainerService _containerService;

        public ListSlidesProvider(IContentManager contentManager, IContainerService containerService)
        {
            _contentManager = contentManager;
            _containerService = containerService;
        }

        public override LocalizedString DisplayName => T("List");

        public override dynamic BuildEditor(dynamic shapeFactory, SlidesProviderContext context)
        {
            return UpdateEditor(shapeFactory, context: context, updater: null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, SlidesProviderContext context, IUpdateModel updater)
        {
            var selectedListId = context.Storage.RetrieveListId();
            var viewModel = new ListSlidesProviderViewModel
            {
                SelectedListId = selectedListId.ToString(),
                DisplayType = context.Storage.RetrieveListSlidesDisplayType()
            };

            if (updater != null)
            {
                if (updater.TryUpdateModel(viewModel, Prefix, null, new[] { "SelectedList" }))
                {
                    selectedListId = ParseInt32Array(viewModel.SelectedListId);
                    context.Storage.StoreListId(selectedListId);
                    context.Storage.StoreListSlidesDisplayType(viewModel.DisplayType.TrimSafe());
                }
            }

            viewModel.SelectedList = selectedListId != null ? _contentManager.Get(selectedListId.Value) : default(ContentItem);
            return shapeFactory.EditorTemplate(TemplateName: "SlidesProviders.List", Model: viewModel, Prefix: Prefix);
        }

        public override IEnumerable<dynamic> BuildSlides(dynamic shapeFactory, SlidesProviderContext context)
        {
            var listId = context.Storage.RetrieveListId();

            if (listId == null)
                yield break;

            var displayType = context.Storage.RetrieveListSlidesDisplayType();
            var contentItems = _containerService.GetContentItems(listId.Value).ToList();

            foreach (var contentItem in contentItems)
            {
                var contentShape = _contentManager.BuildDisplay(contentItem, displayType);
                yield return contentShape;
            }
        }

        public override void Exporting(SlidesProviderExportContext context)
        {
            var displayType = context.Storage.RetrieveListSlidesDisplayType();
            var listId = context.Storage.RetrieveListId();
            var list = listId != null ? _contentManager.Get(listId.Value) : default(ContentItem);
            var listIdentity = list != null ? _contentManager.GetItemMetadata(list).Identity.ToString() : default(string);

            context.Element.SetAttributeValue("List", listIdentity);
            context.Element.SetAttributeValue("DisplayType", displayType);
        }

        public override void Importing(SlidesProviderImportContext context)
        {
            var displayType = context.Element.Attr("DisplayType");
            var listIdentity = context.Element.Attr("List");
            var list = context.GetItemFromSession(listIdentity);

            context.Storage.StoreListId(list?.Id);
            context.Storage.StoreListSlidesDisplayType(displayType);
        }

        private static int? ParseInt32Array(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            var list = value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return list.Any() ? XmlHelper.Parse<int?>(list.First()) : null;
        }
    }
}