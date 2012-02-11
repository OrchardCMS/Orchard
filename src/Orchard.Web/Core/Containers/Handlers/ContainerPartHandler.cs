using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Settings;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Core.Containers.Extensions;
using Orchard.ContentManagement;
using System.Web.Routing;

namespace Orchard.Core.Containers.Handlers {
    public class ContainerPartHandler : ContentHandler {
        public ContainerPartHandler(IRepository<ContainerPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<ContainerPart>((context, part) => {
                part.Record.ItemsShown = true;
                part.Record.PageSize = part.Settings.GetModel<ContainerTypePartSettings>().PageSizeDefault
                                        ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PageSizeDefault;
                part.Record.Paginated = part.Settings.GetModel<ContainerTypePartSettings>().PaginatedDefault
                                        ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PaginatedDefault;

                // hard-coded defaults for ordering
                part.Record.OrderByProperty = part.Is<CommonPart>() ? "CommonPart.CreatedUtc" : string.Empty;
                part.Record.OrderByDirection = (int)OrderByDirection.Descending;
            });
            OnGetContentItemMetadata<ContainerPart>((context, part) => {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", "Containers"},
                {"Controller", "Item"},
                {"Action", "Display"},
                {"id", context.ContentItem.Id}
            };});
        }
    }
}