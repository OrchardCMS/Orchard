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
        public const int PageSizeDefaultDefault = 10;
        public const bool PaginatedDefaultDefault = true;

        private int? _pageSizeDefault;
        private bool? _paginiatedDefault;

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
        public int? PageSizeDefault { get; set; }
        public bool? PaginatedDefault { get; set; }
        public string RestrictedItemContentTypes { get; set; }
        public bool RestrictItemContentTypes { get; set; }
        public bool? EnablePositioning { get; set; }
        public string AdminListViewName { get; set; }
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

            if (model.PageSizeDefault == null)
                model.PageSizeDefault = partModel.PageSizeDefault;

            if (model.PaginatedDefault == null)
                model.PaginatedDefault = partModel.PaginatedDefault;

            var viewModel = new ContainerTypePartSettingsViewModel {
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
            builder.WithSetting("ContainerTypePartSettings.PageSizeDefault", viewModel.PageSizeDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.PaginatedDefault", viewModel.PaginatedDefault.ToString());
            builder.WithSetting("ContainerTypePartSettings.RestrictedItemContentTypes", viewModel.RestrictedItemContentTypes != null ? string.Join(",", viewModel.RestrictedItemContentTypes) : "");
            builder.WithSetting("ContainerTypePartSettings.RestrictItemContentTypes", viewModel.RestrictItemContentTypes.ToString());
            builder.WithSetting("ContainerTypePartSettings.EnablePositioning", viewModel.EnablePositioning.ToString());
            builder.WithSetting("ContainerTypePartSettings.AdminListViewName", viewModel.AdminListViewName);
            yield return DefinitionTemplate(viewModel);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContainerPart")
                yield break;

            var model = new ContainerPartSettings();
            updateModel.TryUpdateModel(model, "ContainerPartSettings", null, null);
            builder.WithSetting("ContainerPartSettings.PageSizeDefault", model.PageSizeDefault.ToString());
            builder.WithSetting("ContainerPartSettings.PaginatedDefault", model.PaginatedDefault.ToString());
            yield return DefinitionTemplate(model);
        }
    }
}
