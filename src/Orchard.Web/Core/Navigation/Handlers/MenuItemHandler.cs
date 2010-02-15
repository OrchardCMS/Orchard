using Orchard.Core.Navigation.Records;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Models {
    public class MenuItemHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;

        public MenuItemHandler(IRepository<MenuItemRecord> repository, IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            Filters.Add(new ActivatingFilter<MenuItem>(MenuItemDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}