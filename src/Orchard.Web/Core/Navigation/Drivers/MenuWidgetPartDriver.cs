using System;
using System.Collections.Generic;
using System.Globalization;
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
                var menu = _menuService.GetMenu(part.Menu.Id);

                if(menu == null) {
                    return null;
                }

                var menuName = menu.As<TitlePart>().Title.HtmlClassify();

                IEnumerable<MenuItem> menuItems = _navigationManager.BuildMenu(menu);

                var routeData = _workContextAccessor.GetContext().HttpContext.Request.RequestContext.RouteData;

                var selectedPath = NavigationHelper.SetSelectedPath(menuItems, routeData);
                                                         ;
                if (part.Breadcrumb) {
                    menuItems = selectedPath;
                    foreach (var menuItem in menuItems) {
                        menuItem.Items = Enumerable.Empty<MenuItem>();
                    }

                    // apply level limites to breadcrumb
                    menuItems = menuItems.Skip(part.StartLevel - 1);
                    if (part.Levels > 0) {
                        menuItems = menuItems.Take(part.Levels);
                    }
                }
                else {
                    IEnumerable<MenuItem> topLevelItems = menuItems.ToList();
                    
                    if(part.StartLevel > 1) {
                        topLevelItems = selectedPath.Where(x => x.Selected);
                    }

                    if (topLevelItems.Any()) {
                        // apply start level by pushing childrens as top level items
                        int i = 0;
                        for (; i < part.StartLevel - 1; i++) {
                            var temp = new List<MenuItem>();
                            foreach (var menuItem in topLevelItems) {
                                temp.AddRange(menuItem.Items);
                            }
                            topLevelItems = temp;
                        }

                        // apply display level
                        if(part.Levels > 0) {
                            var current = topLevelItems.ToList();
                            for (; i < part.Levels - part.StartLevel; i++ ) {
                                var temp = new List<MenuItem>();
                                foreach (var menuItem in current) {
                                    temp.AddRange(menuItem.Items);
                                }
                                current = temp;
                            }
                            foreach(var menuItem in current) {
                                menuItem.Items = Enumerable.Empty<MenuItem>();
                            }
                        }

                        menuItems = topLevelItems;
                    }
                }

                dynamic menuShape = shapeHelper.Menu().MenuName(menuName);
                NavigationHelper.PopulateMenu(shapeHelper, menuShape, menuShape, menuItems);

                return shapeHelper.Parts_MenuWidget(Menu: menuShape);
            });
        }
        
        protected override DriverResult Editor(MenuWidgetPart part, dynamic shapeHelper) {
            return ContentShape("Parts_MenuWidget_Edit", () => {
                    var model = new MenuWidgetViewModel {
                        CurrentMenuId = part.Menu == null ? -1 : part.Menu.Id,
                        StartLevel = part.StartLevel,
                        StopLevel = part.Levels,
                        Breadcrumb = part.Breadcrumb,
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
                part.Menu = _contentManager.Get(model.CurrentMenuId).Record;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(MenuWidgetPart part, ImportContentContext context) {
            context.ImportAttribute(part.PartDefinition.Name, "StartLevel", x => part.StartLevel = Convert.ToInt32(x));
            context.ImportAttribute(part.PartDefinition.Name, "Levels", x => part.Levels = Convert.ToInt32(x));
            context.ImportAttribute(part.PartDefinition.Name, "Breadcrumb", x => part.Breadcrumb = Convert.ToBoolean(x));

            context.ImportAttribute(part.PartDefinition.Name, "Menu", x => part.Menu = context.GetItemFromSession(x).Record);
        }

        protected override void Exporting(MenuWidgetPart part, ExportContentContext context) {
            var menuIdentity = _contentManager.GetItemMetadata(_contentManager.Get(part.Menu.Id)).Identity;
            context.Element(part.PartDefinition.Name).SetAttributeValue("Menu", menuIdentity);

            context.Element(part.PartDefinition.Name).SetAttributeValue("StartLevel", part.StartLevel);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Levels", part.Levels);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Breadcrumb", part.Breadcrumb);
        }

    }
}
