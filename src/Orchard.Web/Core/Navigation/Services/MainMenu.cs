using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Records;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class MainMenu : INavigationProvider {
        private readonly IContentManager _contentManager;

        public MainMenu(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public string MenuName { get { return "mainmenu"; } }

        public void GetNavigation(NavigationBuilder builder) {
            IEnumerable<MenuPart> menuParts = _contentManager.Query<MenuPart, MenuPartRecord>().Where(x => x.AddToMainMenu).List();
            foreach (var menuPart in menuParts) {
                if (menuPart != null ) {
                    MenuPart part = menuPart;
                    builder.Add(menu => menu
                                            .Add(part.MenuText, part.MenuPosition));
                }
            }
        }
    }
}
