using System;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;

        public MenuPartHandler(
            IRepository<MenuPartRecord> menuPartRepository,
            IContentManager contentManager
            ) {

            _contentManager = contentManager;
            Filters.Add(StorageFilter.For(menuPartRepository));

            OnInitializing<MenuPart>((ctx, x) => {
                                      x.MenuText = String.Empty;
                                  });

            OnActivated<MenuPart>(PropertySetHandlers);
        }

        protected void PropertySetHandlers(ActivatedContentContext context, MenuPart menuPart) {
            menuPart.MenuField.Setter(menu => {
                if(menu == null || menu.ContentItem == null) {
                    menuPart.Record.MenuId = 0;
                }
                else {
                    menuPart.Record.MenuId = menu.ContentItem.Id;    
                }
                
                return menu;
            });

            menuPart.MenuField.Loader(ctx =>
                _contentManager.Get(menuPart.Record.MenuId, menuPart.IsPublished() ? VersionOptions.Published : VersionOptions.Latest)
            );
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<MenuPart>();

            if (part != null) {
                string stereotype;
                if (context.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem") {
                    context.Metadata.DisplayText = part.MenuText;    
                }
            }
        }

    }
}