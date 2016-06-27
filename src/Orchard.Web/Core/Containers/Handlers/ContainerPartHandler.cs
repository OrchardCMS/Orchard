using System;
using System.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.Settings;
using Orchard.Data;
using System.Web.Routing;

namespace Orchard.Core.Containers.Handlers {
    public class ContainerPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IListViewService _listViewService;
        private readonly IContainerService _containerService;

        public ContainerPartHandler(
            IRepository<ContainerPartRecord> repository, 
            IContentDefinitionManager contentDefinitionManager, 
            IListViewService listViewService, 
            IContainerService containerService) {

            _contentDefinitionManager = contentDefinitionManager;
            _listViewService = listViewService;
            _containerService = containerService;
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<ContainerPart>((context, part) => {
                part.Record.ItemsShown = part.Settings.GetModel<ContainerTypePartSettings>().ItemsShownDefault
                                        ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().ItemsShownDefault;
                part.Record.PageSize = part.Settings.GetModel<ContainerTypePartSettings>().PageSizeDefault
                                        ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PageSizeDefault;
                part.Record.Paginated = part.Settings.GetModel<ContainerTypePartSettings>().PaginatedDefault
                                        ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PaginatedDefault;

            });

            OnGetContentItemMetadata<ContainerPart>((context, part) => {
                    context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Containers"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"id", context.ContentItem.Id}
                };
            });

            OnActivated<ContainerPart>((context, part) => {
                part.ContainerSettingsField.Loader(() => part.Settings.GetModel<ContainerTypePartSettings>());

                part.ItemContentTypesField.Loader(() => {
                    var settings = part.ContainerSettings;
                    var types = settings.RestrictItemContentTypes ? settings.RestrictedItemContentTypes : part.Record.ItemContentTypes;
                    return _contentDefinitionManager.ParseContentTypeDefinitions(types);
                });

                part.ItemContentTypesField.Setter(value => {
                    part.Record.ItemContentTypes = _contentDefinitionManager.JoinContentTypeDefinitions(value);
                    return value;
                });

                part.AdminListViewField.Loader(() => {
                    var providers = _listViewService.Providers.ToList();
                    var listViewProviderName = !String.IsNullOrWhiteSpace(part.Record.AdminListViewName)
                        ? part.Record.AdminListViewName
                        : !String.IsNullOrWhiteSpace(part.ContainerSettings.AdminListViewName)
                            ? part.ContainerSettings.AdminListViewName
                            : providers.Any() ? providers.First().Name : null;

                    return _listViewService.GetProvider(listViewProviderName) ?? _listViewService.GetDefaultProvider();
                });
            });

            OnPublished<ContainerPart>((context, part) => _containerService.UpdateItemCount(part));
        }
    }
}