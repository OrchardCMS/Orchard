using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuHandler : ContentHandler {
        private readonly IMenuService _menuService;
        private readonly IContentManager _contentManager;

        public MenuHandler(IMenuService menuService, IContentManager contentManager) {
            _menuService = menuService;
            _contentManager = contentManager;
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if(context.ContentItem.ContentType != "Menu") {
                return;
            }

            context.Metadata.Identity.Add("name", context.ContentItem.As<TitlePart>().Title);
        }

        protected override void Removing(RemoveContentContext context) {
            if (context.ContentItem.ContentType != "Menu") {
                return;
            }

            // remove all menu items
            var menuParts = _menuService.GetMenuParts(context.ContentItem.Id);

            foreach(var menuPart in menuParts) {
                _contentManager.Remove(menuPart.ContentItem);
            }
        }
    }
}