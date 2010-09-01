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
            var menuName = "main";
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                menuName = "admin";

            //todo: (heskew) does the menu need to be on Page?
            var shape = _shapeHelperFactory.CreateHelper();
            _workContextAccessor.GetContext(filterContext).CurrentPage.Zones["Navigation"].Add(shape.Menu(_navigationManager.BuildMenu(menuName)));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}