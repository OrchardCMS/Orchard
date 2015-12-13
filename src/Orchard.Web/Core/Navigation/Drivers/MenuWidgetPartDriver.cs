using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Navigation.Drivers {
    public class MenuWidgetPartDriver : ContentPartDriver<MenuWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly INavigationManager _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IMenuService _menuService;

        public MenuWidgetPartDriver(
            IContentManager contentManager,
            INavigationManager navigationManager,
            IWorkContextAccessor workContextAccessor,
            IMenuService menuService) {
            _contentManager = contentManager;
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
            _menuService = menuService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get {
                return "MenuWidget";
            }
        }

        protected override DriverResult Display(MenuWidgetPart part, string displayType, dynamic shapeHelper) {
            return ContentShape( "Parts_MenuWidget", () => {
                var menu = _menuService.GetMenu(part.MenuContentItemId);
                
                if(menu == null) {
                    return null;
                }

                var menuName = menu.As<TitlePart>().Title.HtmlClassify();
                var currentCulture = _workContextAccessor.GetContext().CurrentCulture;
                var menuItems = _navigationManager.BuildMenu(menu);
                var localized = new List<MenuItem>();
                foreach(var menuItem in menuItems) {
                    // if there is no associated content, it as culture neutral
                    if(menuItem.Content == null) {
                        localized.Add(menuItem);
                    }

                    // if the menu item is culture neutral or of the current culture
                    else if (String.IsNullOrEmpty(menuItem.Culture) || String.Equals(menuItem.Culture, currentCulture, StringComparison.OrdinalIgnoreCase)) {
                        localized.Add(menuItem);
                    }
                }

                menuItems = localized;

                var request = _workContextAccessor.GetContext().HttpContext.Request;
                var routeData = request.RequestContext.RouteData;
                var selectedPath = NavigationHelper.SetSelectedPath(menuItems, request, routeData);                            
                var menuShape = shapeHelper.Menu();

                if (part.Breadcrumb) {
                    menuItems = selectedPath ?? new Stack<MenuItem>();
                    foreach (var menuItem in menuItems) {
                        menuItem.Items = Enumerable.Empty<MenuItem>();
                    }

                    // apply level limits to breadcrumb
                    menuItems = menuItems.Skip(part.StartLevel - 1);
                    if (part.Levels > 0) {
                        menuItems = menuItems.Take(part.Levels);
                    }

                    var result = new List<MenuItem>(menuItems);

                    // inject the home page
                    if (part.AddHomePage) {
                        result.Insert(0, new MenuItem {
                            Href = _navigationManager.GetUrl("~/", null),
                            Text = T("Home")
                        });
                    }

                    // inject the current page
                    if (!part.AddCurrentPage && selectedPath != null) {
                        result.RemoveAt(result.Count - 1);
                    }

                    // prevent the home page to be added as the home page and the current page
                    if (result.Count == 2 && String.Equals(result[0].Href, result[1].Href, StringComparison.OrdinalIgnoreCase)) {
                        result.RemoveAt(1);
                    }

                    menuItems = result;
                    menuShape = shapeHelper.Breadcrumb();
                }
                else {
                    var topLevelItems = menuItems.ToList();

                    // apply start level by pushing children as top level items. When the start level is
                    // greater than 1 (ie. below the top level), only menu items along the selected path
                    // will be displayed.
                    for (var i = 0; topLevelItems.Any() && i < part.StartLevel - 1; i++) {
                        var temp = new List<MenuItem>();
                        // should the menu be filtered on the currently displayed page ?
                        if (part.ShowFullMenu) {
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

                    // limit the number of levels to display (down from and including the start level)
                    if(part.Levels > 0) {
                        var current = topLevelItems.ToList();
                        for (var i = 1; current.Any() && i < part.Levels; i++ ) {
                            var temp = new List<MenuItem>();
                            foreach (var menuItem in current) {
                                temp.AddRange(menuItem.Items);
                            }
                            current = temp;
                        }
                        // cut the sub-levels beneath any menu items that are at the lowest level being displayed
                        foreach (var menuItem in current) {
                            menuItem.Items = Enumerable.Empty<MenuItem>();
                        }                
                    }
                    menuItems = topLevelItems;
                }

                menuShape.MenuName(menuName);
                menuShape.ContentItem(menu);

                NavigationHelper.PopulateMenu(shapeHelper, menuShape, menuShape, menuItems);

                return shapeHelper.Parts_MenuWidget(Menu: menuShape);
            });
        }
        
        protected override DriverResult Editor(MenuWidgetPart part, dynamic shapeHelper) {
            return ContentShape("Parts_MenuWidget_Edit", () => {

                    var model = new MenuWidgetViewModel {
                        CurrentMenuId = part.MenuContentItemId,
                        StartLevel = part.StartLevel,
                        StopLevel = part.Levels,
                        Breadcrumb = part.Breadcrumb,
                        AddCurrentPage = part.AddCurrentPage,
                        AddHomePage = part.AddHomePage,
                        ShowFullMenu = part.ShowFullMenu,
                        Menus = _menuService.GetMenus(),
                    };

                    return shapeHelper.EditorTemplate(TemplateName: "Parts.MenuWidget.Edit", Model: model, Prefix: Prefix);
                });
        }

        protected override DriverResult Editor(MenuWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MenuWidgetViewModel();

            if(updater.TryUpdateModel(model, Prefix, null, null)) {
                part.StartLevel = model.StartLevel;
                part.Levels = model.StopLevel;
                part.Breadcrumb = model.Breadcrumb;
                part.AddHomePage = model.AddHomePage;
                part.AddCurrentPage = model.AddCurrentPage;
                part.ShowFullMenu = model.ShowFullMenu;
                part.MenuContentItemId = model.CurrentMenuId;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(MenuWidgetPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "StartLevel", x => part.StartLevel = Convert.ToInt32(x));
            context.ImportAttribute(part.PartDefinition.Name, "Levels", x => part.Levels = Convert.ToInt32(x));
            context.ImportAttribute(part.PartDefinition.Name, "Breadcrumb", x => part.Breadcrumb = Convert.ToBoolean(x));
            context.ImportAttribute(part.PartDefinition.Name, "AddCurrentPage", x => part.AddCurrentPage = Convert.ToBoolean(x));
            context.ImportAttribute(part.PartDefinition.Name, "AddHomePage", x => part.AddHomePage = Convert.ToBoolean(x));
            context.ImportAttribute(part.PartDefinition.Name, "ShowFullMenu", x => part.ShowFullMenu = Convert.ToBoolean(x));

            context.ImportAttribute(part.PartDefinition.Name, "Menu", x => part.MenuContentItemId = context.GetItemFromSession(x).Id);
        }

        protected override void Exporting(MenuWidgetPart part, ExportContentContext context) {
            var menuIdentity = _contentManager.GetItemMetadata(_contentManager.Get(part.MenuContentItemId)).Identity;
            context.Element(part.PartDefinition.Name).SetAttributeValue("Menu", menuIdentity);

            context.Element(part.PartDefinition.Name).SetAttributeValue("StartLevel", part.StartLevel);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Levels", part.Levels);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Breadcrumb", part.Breadcrumb);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AddCurrentPage", part.AddCurrentPage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AddHomePage", part.AddHomePage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ShowFullMenu", part.ShowFullMenu);
        }

    }
}
