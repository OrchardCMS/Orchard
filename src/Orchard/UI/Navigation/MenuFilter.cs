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
            var baseViewModel = BaseViewModel.From(filterContext.Result);
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