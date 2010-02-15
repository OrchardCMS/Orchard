using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Navigation;

namespace Orchard.UI.Menus {
    public class AdminMenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;

        public AdminMenuFilter(INavigationManager navigationManager ) {
            _navigationManager = navigationManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var adminViewModel = viewResult.ViewData.Model as AdminViewModel;
            if (adminViewModel == null)
                return;

            adminViewModel.AdminMenu = _navigationManager.BuildMenu("admin");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
            
        }
    }
}
