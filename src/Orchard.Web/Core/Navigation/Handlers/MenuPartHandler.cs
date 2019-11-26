using System;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    public class MenuPartHandler : ContentHandler {
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;

        public MenuPartHandler(
            IRepository<MenuPartRecord> menuPartRepository,
            IContentManager contentManager,
            ISignals signals
            ) {

            _contentManager = contentManager;
            _signals = signals;

            Filters.Add(StorageFilter.For(menuPartRepository));

            OnInitializing<MenuPart>((ctx, x) => {
                x.MenuText = String.Empty;
            });

            OnActivated<MenuPart>(PropertySetHandlers);

            // all items in a menu are characterized by having a MenuPart
            // invalidating the NavigationManager caches when those change may be enough
            OnUpdated<MenuPart>((context, part) => InvalidateNavCache());
            OnPublished<MenuPart>((context, part) => InvalidateNavCache());
            OnRemoved<MenuPart>((context, part) => InvalidateNavCache());
            OnDestroyed<MenuPart>((context, part) => InvalidateNavCache());
        }

        protected void PropertySetHandlers(ActivatedContentContext context, MenuPart menuPart) {
            menuPart.MenuField.Setter(menu => {
                if (menu == null || menu.ContentItem == null) {
                    menuPart.Record.MenuId = 0;
                }
                else {
                    menuPart.Record.MenuId = menu.ContentItem.Id;
                }

                return menu;
            });

            menuPart.MenuField.Loader(() =>
                _contentManager.Get(menuPart.Record.MenuId, menuPart.IsPublished() ? VersionOptions.Published : VersionOptions.Latest)
            );
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<MenuPart>();

            if (part != null) {
                string stereotype;
                if (context.ContentItem.TypeDefinition.Settings.TryGetValue("Stereotype", out stereotype) && stereotype == "MenuItem") {
                    context.Metadata.DisplayText = part.MenuText;
                    context.Metadata.EditorRouteValues = new RouteValueDictionary {
                        {"Area", "Navigation"},
                        {"Controller", "Admin"},
                        {"Action", "Edit"},
                        {"Id", context.ContentItem.Id}
                    };
                    context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                        {"Area", "Navigation"},
                        {"Controller", "Admin"},
                        {"Action", "Delete"},
                        {"Id", context.ContentItem.Id}
                    };
                }
            }
        }

        private void InvalidateNavCache() {
            _signals.Trigger("Orchard.Core.Navigation.MenusUpdated");
        }
    }
}