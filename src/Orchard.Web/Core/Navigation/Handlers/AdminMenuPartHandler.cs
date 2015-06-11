using System;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class AdminMenuPartHandler : ContentHandler {
        public AdminMenuPartHandler(IRepository<AdminMenuPartRecord> menuPartRepository) {
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnInitializing<AdminMenuPart>((ctx, x) => {
                                      x.OnAdminMenu = false;
                                      x.AdminMenuText = String.Empty;
                                  });
        }
    }
}