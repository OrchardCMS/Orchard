using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Title.Models;

namespace Orchard.Core.Navigation.Services
{
    public class MainMenuService : IMenuService
    {
        private readonly IContentManager _contentManager;
        private ICacheManager _cacheManager;
        private ISignals _signals;

        public MainMenuService(IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals)
        {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<MenuPart> Get()
        {
            return _contentManager.Query<MenuPart, MenuPartRecord>().List();
        }

        public IEnumerable<MenuPart> GetMenuParts(int menuId)
        {
            return _contentManager
                .Query<MenuPart, MenuPartRecord>()
                .Where(x => x.MenuId == menuId)
                .List();
        }

        public IContent GetMenu(string menuName)
        {
            if (string.IsNullOrWhiteSpace(menuName))
            {
                return null;
            }

            return _contentManager.Query<TitlePart, TitlePartRecord>()
                .Where(x => x.Title == menuName)
                .ForType("Menu")
                .Slice(0, 1)
                .FirstOrDefault();
        }

        public IContent GetMenu(int menuId)
        {
            return _contentManager.Get(menuId, VersionOptions.Published);
        }

        public MenuPart Get(int menuPartId)
        {
            return _contentManager.Get<MenuPart>(menuPartId);
        }

        public IContent Create(string name)
        {

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name);
            }

            var menu = _contentManager.Create("Menu");
            menu.As<TitlePart>().Title = name;

            _signals.Trigger("MainMenuService.AllMenus");

            return menu;
        }

        public void Delete(MenuPart menuPart)
        {
            _contentManager.Remove(menuPart.ContentItem);
        }

        private IEnumerable<ContentItem> AllMenus {
            get {
                var fromCache = FromCache();
                if (fromCache.Item1.AddMinutes(2) < DateTime.Now)
                {
                    // give 2 minutes lifetime to cache
                    _signals.Trigger("MainMenuService.AllMenus");
                    fromCache = FromCache();
                }
                return fromCache.Item2;
            }
        }
        private Tuple<DateTime, IEnumerable<ContentItem>> FromCache()
        {
            return _cacheManager
                    .Get<string, Tuple<DateTime, IEnumerable<ContentItem>>>("MainMenuService.AllMenus", true, ctx =>
                    {
                        ctx.Monitor(_signals.When("MainMenuService.AllMenus"));

                        return Tuple.Create(DateTime.Now,
                            _contentManager.Query().ForType("Menu").Join<TitlePartRecord>().OrderBy(x => x.Title).List());
                    });
        }

        public IEnumerable<ContentItem> GetMenus()
        {
            return AllMenus; // _contentManager.Query().ForType("Menu").Join<TitlePartRecord>().OrderBy(x => x.Title).List();
        }
    }
}