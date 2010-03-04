using System;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuPartHandler : ContentHandler {
        public MenuPartHandler(IRepository<MenuPartRecord> menuPartRepository) {
            Filters.Add(new ActivatingFilter<MenuPart>("blog"));
            Filters.Add(new ActivatingFilter<MenuPart>("page"));
            Filters.Add(new ActivatingFilter<MenuPart>("menuitem"));
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnActivated<MenuPart>((ctx, x) => {
                                      x.OnMainMenu = false;
                                      x.MenuText = String.Empty;
                                  });
        }
    }
}