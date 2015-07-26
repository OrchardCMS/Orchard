using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.ViewModels;

namespace Orchard.Core.Containers.Settings {
    public class ContainerPartSettings {
        public const bool ItemsShownDefaultDefault = true;
        public const int PageSizeDefaultDefault = 10;
        public const bool PaginatedDefaultDefault = true;
        public const string ItemsDisplayModelDefault = "Summary";
        public const string ItemTagDefault = "li";
        public const string ContainerTagDefault = "ul";

        private bool? _itemsShownDefault;
        private int? _pageSizeDefault;
        private bool? _paginiatedDefault;
        private string _itemsDisplayMode;
        private string _itemTag = "li";
        private string _containerTag = "ul";

        public string ContainerTag
        {
            get
            {
                return !string.IsNullOrEmpty(_containerTag)
                         ? _containerTag
                         : ContainerTagDefault;
            }
            set { _containerTag = value; }
        }
        
        public string ItemTag
        {
            get
            {
                return !string.IsNullOrEmpty(_itemTag)
                         ? _itemTag
                         : ItemTagDefault;
            }
            set { _itemTag = value; }
        }
        
        public string ItemsDisplayMode
        {
            get
            {
                return !string.IsNullOrEmpty(_itemsDisplayMode)
                         ? _itemsDisplayMode
                         : ItemsDisplayModelDefault;
            }
            set { _itemsDisplayMode = value; }
        }

        public bool ItemsShownDefault {
            get {
                return _itemsShownDefault != null
                         ? (bool)_itemsShownDefault
                         : ItemsShownDefaultDefault;
            }
            set { _itemsShownDefault = value; }
        }

        public int PageSizeDefault {
            get {
                return _pageSizeDefault != null
                         ? (int)_pageSizeDefault
                         : PageSizeDefaultDefault;
            }
            set { _pageSizeDefault = value; }
        }

        public bool PaginatedDefault {
            get {
                return _paginiatedDefault != null
                         ? (bool)_paginiatedDefault
                         : PaginatedDefaultDefault;
            }
            set { _paginiatedDefault = value; }
        }
    }

    public class ContainerTypePartSettings {
        public bool? ItemsShownDefault { get; set; }
        public int? PageSizeDefault { get; set; }
        public bool? PaginatedDefault { get; set; }
        public string RestrictedItemContentTypes { get; set; }
        public bool RestrictItemContentTypes { get; set; }
        public bool? EnablePositioning { get; set; }
        public string AdminListViewName { get; set; }
        public string ItemsDisplayMode { get; set; }
        public string ContainerTag { get; set; }
        public string ItemTag { get; set; }
    }

    public class ContainerSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly IContainerService _containerService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IListViewService _listViewService;

        public ContainerSettingsHooks(IContainerService containerService, IContentDefinitionManager contentDefinitionManager, IListViewService listViewService) {
            _containerService = containerService;
            _contentDefinitionManager = contentDefinitionManager;
            _listViewService = listViewService;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ContainerPart")
                yield break;

            var model = definition.Settings.GetModel<ContainerTypePartSettings>();
            var partModel = definition.PartDefinition.Settings.GetModel<ContainerPartSettings>();
            
            if (model.ItemsShownDefault == null)
                model.ItemsShownDefault = partModel.ItemsShownDefault;

            if (model.PageSizeDefault == null)
                model.PageSizeDefault = partModel.PageSizeDefault;

            if (model.PaginatedDefault == null)
                model.PaginatedDefault = partModel.PaginatedDefault;

            if (string.IsNullOrEmpty(model.ItemsDisplayMode))
            {
                model.ItemsDisplayMode = partModel.ItemsDisplayMode;
            }

            if (string.IsNullOrEmpty(model.ContainerTag))
            {
                model.ContainerTag = partModel.ContainerTag;
            }

            if (string.IsNullOrEmpty(model.ItemTag))
            {
                model.ItemTag = partModel.ItemTag;
            }
            
            var viewModel = new ContainerTypePartSettingsViewModel
            {
                ItemTag = model.ItemTag,
                ContainerTag = model.ContainerTag,
                ItemsDisplayMode = model.ItemsDisplayMode,
                ItemsShownDefault = model.ItemsShownDefault,
                PageSizeDefault = model.PageSizeDefault,
                PaginatedDefault = model.PaginatedDefault,
                RestrictedItemContentTypes = _contentDefinitionManager.ParseContentTypeDefinitions(model.RestrictedItemContentTypes).Select(x => x.Name).ToList(),
                RestrictItemContentTypes = model.RestrictItemContentTypes,
                EnablePositioning = model.EnablePositioning,
                AdminListViewName = model.AdminListViewName,
                AvailableItemContentTypes = _containerService.GetContainableTypes().ToList(),
                ListViewProviders = _listViewService.Providers.ToList()
            };

            yield return DefinitionTemplate(viewModel);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            if (definition.Name != "ContainerPart")
                yield break;

            var model = definition.Settings.GetModel<ContainerPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContainerPart")
                yield break;

            var viewModel = new ContainerTypePartSettingsViewModel {
                AvailableItemContentTypes = _containerService.GetContainableTypes().ToList()
            };
            updateModel.TryUpdateModel(viewModel, "ContainerTypePartSettingsViewModel", null, new[] { "AvailableItemContentTypes" });
            builder.WithSetting("ContainerTypePartSettings.ItemsShownDefault", viewModel.ItemsShownDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.PageSizeDefault", viewModel.PageSizeDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.PaginatedDefault", viewModel.PaginatedDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.RestrictedItemContentTypes", viewModel.RestrictedItemContentTypes != null ? string.Join(",", viewModel.RestrictedItemContentTypes) : "");
            builder.WithSetting("ContainerTypePartSettings.RestrictItemContentTypes", viewModel.RestrictItemContentTypes.ToString());
            builder.WithSetting("ContainerTypePartSettings.EnablePositioning", viewModel.EnablePositioning.ToString());
            builder.WithSetting("ContainerTypePartSettings.AdminListViewName", viewModel.AdminListViewName);
            builder.WithSetting("ContainerTypePartSettings.ItemsDisplayMode", viewModel.ItemsDisplayMode);
            builder.WithSetting("ContainerTypePartSettings.ContainerTag", viewModel.ContainerTag);
            builder.WithSetting("ContainerTypePartSettings.ItemTag", viewModel.ItemTag);
            yield return DefinitionTemplate(viewModel);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContainerPart")
                yield break;

            var model = new ContainerPartSettings();
            updateModel.TryUpdateModel(model, "ContainerPartSettings", null, null);
            builder.WithSetting("ContainerPartSettings.ItemsShownDefault", model.ItemsShownDefault.ToString());
            builder.WithSetting("ContainerPartSettings.PageSizeDefault", model.PageSizeDefault.ToString());
            builder.WithSetting("ContainerPartSettings.PaginatedDefault", model.PaginatedDefault.ToString());
            builder.WithSetting("ContainerPartSettings.ItemsDisplayMode", model.ItemsDisplayMode);
            builder.WithSetting("ContainerPartSettings.ContainerTag", model.ContainerTag);
            builder.WithSetting("ContainerPartSettings.ItemTag", model.ItemTag);
            yield return DefinitionTemplate(model);
        }
    }
}
