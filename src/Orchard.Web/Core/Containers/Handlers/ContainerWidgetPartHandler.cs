using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Core.Containers.Handlers {
    public class ContainerWidgetPartHandler : ContentHandler {
        public ContainerWidgetPartHandler(IRepository<ContainerWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<ContainerWidgetPart>((context, part) => {
                part.Record.ContainerId = 0;
                part.Record.PageSize = 5;
            });
        }
    }
}