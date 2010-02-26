using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Admin;

namespace Orchard.UI.Navigation {
    public class MenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;

        public MenuFilter(INavigationManager navigationManager) {
            _navigationManager = navigationManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var baseViewModel = viewResult.ViewData.Model as BaseViewModel;
            if (baseViewModel == null)
                return;

            var menuName = "main";
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                menuName = "admin";

            baseViewModel.Menu = _navigationManager.BuildMenu(menuName);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}