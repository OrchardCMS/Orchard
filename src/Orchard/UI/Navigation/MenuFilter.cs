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
        private readonly IShapeHelperFactory _shapeHelperFactory;

        public MenuFilter(INavigationManager navigationManager, IWorkContextAccessor workContextAccessor, IShapeHelperFactory shapeHelperFactory) {
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
            _shapeHelperFactory = shapeHelperFactory;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var workContext = _workContextAccessor.GetContext(filterContext);
            var shape = _shapeHelperFactory.CreateHelper();

            var menuName = "main";
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                menuName = "admin";

            var menuItems = _navigationManager.BuildMenu(menuName);

            var menuShape = shape.Menu().MenuName(menuName);
            PopulateMenu(shape, menuShape, menuItems);

            workContext.Page.Navigation.Add(menuShape);
        }

        private void PopulateMenu(dynamic shape, dynamic parentShape, IEnumerable<MenuItem> menuItems) {
            foreach (var menuItem in menuItems) {
                var menuItemShape = shape.MenuItem()
                    .Text(menuItem.Text)
                    .Href(menuItem.Href)
                    .RouteValues(menuItem.RouteValues)
                    .Item(menuItem);
                
                if (menuItem.Items != null && menuItem.Items.Any()) {
                    PopulateMenu(shape, menuItemShape, menuItem.Items);
                }

                parentShape.Add(menuItemShape, menuItem.Position);
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) { }
    }
}