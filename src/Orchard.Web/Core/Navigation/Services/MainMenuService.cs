using System.Collections.Generic;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.Services {
    [UsedImplicitly]
    public class MainMenuService : IMenuService {
        private readonly IContentManager _contentManager;

        public MainMenuService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<MenuPart> Get() {
            return _contentManager.Query<MenuPart, MenuPartRecord>().Where(x => x.OnMainMenu).List();
        }

        public MenuPart Get(int menuPartId) {
            return _contentManager.Get<MenuPart>(menuPartId);
        }

        public void Delete(MenuPart menuPart) {
            _contentManager.Remove(menuPart.ContentItem);
        }
    }
}