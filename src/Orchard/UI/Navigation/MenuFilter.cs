using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Admin;

namespace Orchard.UI.Navigation {
    public class MenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public MenuFilter(INavigationManager navigationManager, IWorkContextAccessor workContextAccessor) {
            _navigationManager = navigationManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
#if REFACTORING
            var baseViewModel = BaseViewModel.From(filterContext.Result);
            if (baseViewModel == null)
                return;

            var menuName = "main";
            if (AdminFilter.IsApplied(filterContext.RequestContext))
                menuName = "admin";

            baseViewModel.Menu = _navigationManager.BuildMenu(menuName);
#endif
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}