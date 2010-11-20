using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;
using Orchard.UI.Admin;

namespace Orchard.UI.Navigation {
    public class MenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public MenuFilter(
            INavigationManager navigationManager, 
            IWorkContextAccessor workContextAccessor, 
            IShapeFactory shapeFactory) {
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);

            var menuName = "main";
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                menuName = "admin";

            var menuItems = _navigationManager.BuildMenu(menuName);

            var menuShape = _shapeFactory.Menu().MenuName(menuName);
            PopulateMenu(_shapeFactory, menuShape, menuShape, menuItems);

            workContext.Layout.Navigation.Add(menuShape);
        }

        private void PopulateMenu(dynamic shapeFactory, dynamic parentShape, dynamic menu, IEnumerable<MenuItem> menuItems) {

            foreach (var menuItem in menuItems) {
                var menuItemShape = shapeFactory.MenuItem()
                    .Text(menuItem.Text)
                    .Href(menuItem.Href)
                    .LinkToFirstChild(menuItem.LinkToFirstChild)
                    .RouteValues(menuItem.RouteValues)
                    .Item(menuItem)
                    .Menu(menu)
                    .Parent(parentShape);
                
                if (menuItem.Items != null && menuItem.Items.Any()) {
                    PopulateMenu(shapeFactory, menuItemShape, menu, menuItem.Items);
                }

                parentShape.Add(menuItemShape, menuItem.Position);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}