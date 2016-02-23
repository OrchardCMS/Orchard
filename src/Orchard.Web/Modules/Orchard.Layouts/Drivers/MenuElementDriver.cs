using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Drivers {
    [OrchardFeature("Orchard.Layouts.UI")]
    public class MenuElementDriver : ElementDriver<Menu> {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IContentManager _contentManager;

        public MenuElementDriver(
            IWorkContextAccessor workContextAccessor,
            IMenuService menuService,
            INavigationManager navigationManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory) {
            _workContextAccessor = workContextAccessor;
            _menuService = menuService;
            _navigationManager = navigationManager;
            _contentManager = contentManager;
            New = shapeFactory;
        }

        public dynamic New { get; set; }

        protected override EditorResult OnBuildEditor(Menu element, ElementEditorContext context) {
            var viewModel = new MenuEditorViewModel {
                CurrentMenuId = element.MenuContentItemId,
                StartLevel = element.StartLevel,
                StopLevel = element.Levels,
                ShowFullMenu = element.ShowFullMenu,
                Menus = _menuService.GetMenus(),
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Menu", Model: viewModel);

            if (context.Updater != null) {
                if (context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null)) {
                    element.StartLevel = viewModel.StartLevel;
                    element.Levels = viewModel.StopLevel;
                    element.ShowFullMenu = viewModel.ShowFullMenu;
                    element.MenuContentItemId = viewModel.CurrentMenuId;
                }
            }

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Menu element, ElementDisplayingContext context) {
            var menu = _menuService.GetMenu(element.MenuContentItemId);

            if (menu == null)
                return;

            var menuName = menu.As<TitlePart>().Title.HtmlClassify();
            var currentCulture = _workContextAccessor.GetContext().CurrentCulture;
            var menuItems = _navigationManager.BuildMenu(menu);
            var localized = new List<MenuItem>();
            foreach (var menuItem in menuItems) {
                // If there is no associated content, it as culture neutral.
                if (menuItem.Content == null)
                    localized.Add(menuItem);

                // If the menu item is culture neutral or of the current culture.
                else if (String.IsNullOrEmpty(menuItem.Culture) || String.Equals(menuItem.Culture, currentCulture, StringComparison.OrdinalIgnoreCase))
                    localized.Add(menuItem);
            }

            menuItems = localized;

            var shapeHelper = New;
            var request = _workContextAccessor.GetContext().HttpContext.Request;
            var routeData = request.RequestContext.RouteData;
            var selectedPath = NavigationHelper.SetSelectedPath(menuItems, request, routeData);
            var menuShape = shapeHelper.Menu();
            var topLevelItems = menuItems.ToList();

            // Apply start level by pushing children as top level items. When the start level is
            // greater than 1 (ie. below the top level), only menu items along the selected path
            // will be displayed.
            for (var i = 0; topLevelItems.Any() && i < element.StartLevel - 1; i++) {
                var temp = new List<MenuItem>();
                // Should the menu be filtered on the currently displayed page?
                if (element.ShowFullMenu) {
                    foreach (var menuItem in topLevelItems) {
                        temp.AddRange(menuItem.Items);
                    }
                }
                else if (selectedPath != null) {
                    topLevelItems = topLevelItems.Intersect(selectedPath.Where(x => x.Selected)).ToList();
                    foreach (var menuItem in topLevelItems) {
                        temp.AddRange(menuItem.Items);
                    }
                }
                topLevelItems = temp;
            }

            // Limit the number of levels to display (down from and including the start level).
            if (element.Levels > 0) {
                var current = topLevelItems.ToList();
                for (var i = 1; current.Any() && i < element.Levels; i++) {
                    var temp = new List<MenuItem>();
                    foreach (var menuItem in current) {
                        temp.AddRange(menuItem.Items);
                    }
                    current = temp;
                }
                // Cut the sub-levels beneath any menu items that are at the lowest level being displayed.
                foreach (var menuItem in current) {
                    menuItem.Items = Enumerable.Empty<MenuItem>();
                }
            }

            menuItems = topLevelItems;
            menuShape.MenuName(menuName);
            menuShape.ContentItem(menu);

            NavigationHelper.PopulateMenu(shapeHelper, menuShape, menuShape, menuItems);

            context.ElementShape.Menu = menuShape;
        }

        protected override void OnExporting(Menu element, ExportElementContext context) {
            var menu = _contentManager.Get(element.MenuContentItemId);
            var menuIdentity = menu != null ? _contentManager.GetItemMetadata(menu).Identity.ToString() : default(string);

            if (menuIdentity != null)
                context.ExportableData["MenuId"] = menuIdentity;
        }

        protected override void OnImporting(Menu element, ImportElementContext context) {
            var menuIdentity = context.ExportableData.Get("MenuId");
            var menu = menuIdentity != null ? context.Session.GetItemFromSession(menuIdentity) : default(ContentManagement.ContentItem);

            if (menu == null)
                return;

            element.MenuContentItemId = menu.Id;
        }
    }
}