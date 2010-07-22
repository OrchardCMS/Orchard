using System;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuPartHandler : ContentHandler {
        public MenuPartHandler(IRepository<MenuPartRecord> menuPartRepository) {
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnInitializing<MenuPart>((ctx, x) => {
                                      x.OnMainMenu = false;
                                      x.MenuText = String.Empty;
                                  });
        }
    }
}