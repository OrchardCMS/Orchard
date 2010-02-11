using System;
using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Records;
using Orchard.Data;

namespace Orchard.Core.Navigation.Models {
    [UsedImplicitly]
    public class MenuPartHandler : ContentHandler {
        public MenuPartHandler(IRepository<MenuPartRecord> menuPartRepository) {
            Filters.Add(new ActivatingFilter<MenuPart>("blog"));
            Filters.Add(new ActivatingFilter<MenuPart>("page"));
            Filters.Add(new ActivatingFilter<MenuPart>("menuitem"));
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnActivated<MenuPart>((ctx, x) => {
                x.AddToMainMenu = false;
                x.MenuText = String.Empty;
            });
        }
    }
}
