using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class MainMenuFilter : FilterProvider, IResultFilter {
        private readonly INavigationManager _navigationManager;

        public MainMenuFilter(INavigationManager navigationManager) {
            _navigationManager = navigationManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult == null)
                return;

            var baseViewModel = viewResult.ViewData.Model as BaseViewModel;
            if (baseViewModel == null)
                return;

            baseViewModel.Menu = _navigationManager.BuildMenu("mainmenu");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}
