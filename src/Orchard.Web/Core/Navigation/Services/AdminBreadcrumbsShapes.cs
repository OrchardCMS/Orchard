using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class AdminBreadcrumbsShapes : IShapeTableProvider {
        private readonly Work<INavigationManager> _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminBreadcrumbsShapes(Work<INavigationManager> navigationManager, IWorkContextAccessor workContextAccessor) {
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("AdminBreadcrumbs").OnDisplaying(context => {
                var menuName = (string)context.Shape.MenuName;
                var menuItems = _navigationManager.Value.BuildMenu(menuName);
                var request = _workContextAccessor.GetContext().HttpContext.Request;
                var routeData = request.RequestContext.RouteData;

                var selectedPath = NavigationHelper.SetSelectedPath(menuItems, request, routeData);
                context.Shape.MenuItems = selectedPath;
            });
        }

        private IEnumerable<MenuItem> Flatten(IEnumerable<MenuItem> menuItems) {
            foreach(var item in menuItems) {
                yield return item;

                if (item.Items.Any()) {
                    foreach(var subItem in Flatten(item.Items)) {
                        yield return subItem;
                    }
                }
            }
        }
    }
}