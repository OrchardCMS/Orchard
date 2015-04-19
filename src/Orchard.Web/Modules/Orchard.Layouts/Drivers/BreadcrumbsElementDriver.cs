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
using Orchard.Layouts.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Layouts.Drivers {
    [OrchardFeature("Orchard.Layouts.UI")]
    public class BreadcrumbsElementDriver : ElementDriver<Breadcrumbs> {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;

        public BreadcrumbsElementDriver(IMenuService menuService, INavigationManager navigationManager, IWorkContextAccessor workContextAccessor, IShapeFactory shapeFactory) {
            _workContextAccessor = workContextAccessor;
            _menuService = menuService;
            _navigationManager = navigationManager;
            New = shapeFactory;
        }

        public dynamic New { get; set; }

        protected override EditorResult OnBuildEditor(Breadcrumbs element, ElementEditorContext context) {
            var viewModel = new BreadcrumbsEditorViewModel {
                CurrentMenuId = element.MenuContentItemId,
                StartLevel = element.StartLevel,
                StopLevel = element.Levels,
                AddCurrentPage = element.AddCurrentPage,
                AddHomePage = element.AddHomePage,
                Menus = _menuService.GetMenus(),
            };
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Breadcrumbs", Model: viewModel);

            if (context.Updater != null) {
                if (context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null)) {
                    element.StartLevel = viewModel.StartLevel;
                    element.Levels = viewModel.StopLevel;
                    element.AddCurrentPage = viewModel.AddCurrentPage;
                    element.AddHomePage = viewModel.AddHomePage;
                    element.MenuContentItemId = viewModel.CurrentMenuId;
                }
            }

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Breadcrumbs element, ElementDisplayingContext context) {
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


            menuItems = selectedPath ?? new Stack<MenuItem>();
            foreach (var menuItem in menuItems) {
                menuItem.Items = Enumerable.Empty<MenuItem>();
            }

            // Apply level limits to breadcrumb.
            menuItems = menuItems.Skip(element.StartLevel - 1);
            if (element.Levels > 0) {
                menuItems = menuItems.Take(element.Levels);
            }

            var result = new List<MenuItem>(menuItems);

            // Inject the home page.
            if (element.AddHomePage) {
                result.Insert(0, new MenuItem {
                    Href = _navigationManager.GetUrl("~/", null),
                    Text = T("Home")
                });
            }

            // Inject the current page.
            if (!element.AddCurrentPage && selectedPath != null) {
                result.RemoveAt(result.Count - 1);
            }

            // Prevent the home page to be added as the home page and the current page.
            if (result.Count == 2 && String.Equals(result[0].Href, result[1].Href, StringComparison.OrdinalIgnoreCase)) {
                result.RemoveAt(1);
            }

            menuItems = result;
            menuShape = shapeHelper.Breadcrumb();
            menuShape.MenuName(menuName);
            menuShape.ContentItem(menu);

            NavigationHelper.PopulateMenu(shapeHelper, menuShape, menuShape, menuItems);

            context.ElementShape.Menu = menuShape;
        }
    }
}