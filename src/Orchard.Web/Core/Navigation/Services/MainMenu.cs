using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Records;
using Orchard.UI.Navigation;
using MenuItem=Orchard.Core.Navigation.Models.MenuItem;

namespace Orchard.Core.Navigation.Services {
    public class MainMenu : INavigationProvider, IMenuService {
        private readonly IContentManager _contentManager;

        public MainMenu(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public string MenuName { get { return "main"; } }

        public IEnumerable<MenuPart> Get() {
            return _contentManager.Query<MenuPart, MenuPartRecord>().Where(x => x.OnMainMenu).List();
        }

        public MenuPart Get(int menuPartId) {
            return _contentManager.Get<MenuPart>(menuPartId);
        }

        public void Delete(MenuPart menuPart) {
            _contentManager.Remove(menuPart.ContentItem);
        }

        public void GetNavigation(NavigationBuilder builder) {
            IEnumerable<MenuPart> menuParts = _contentManager.Query<MenuPart, MenuPartRecord>().Where(x => x.OnMainMenu).List();
            foreach (var menuPart in menuParts) {
                if (menuPart != null ) {
                    MenuPart part = menuPart;

                    if (part.Is<MenuItem>())
                        builder.Add(
                            menu => menu.Add(part.MenuText, part.MenuPosition, nib => nib.Url(part.As<MenuItem>().Url)));
                    else
                        builder.Add(
                            menu =>
                            menu.Add(part.MenuText, part.MenuPosition,
                                     nib =>
                                     nib.Action(_contentManager.GetItemMetadata(part.ContentItem).DisplayRouteValues)));
                }
            }
        }
    }

    public interface IMenuService : IDependency {
        IEnumerable<MenuPart> Get();
        MenuPart Get(int menuPartId);
        void Delete(MenuPart menuPart);
    }
}
