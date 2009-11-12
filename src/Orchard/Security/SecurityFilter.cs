using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.Security {
    public class SecurityFilter : FilterProvider, IResultFilter {
        private readonly IAuthenticationService _authenticationService;

        public SecurityFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var viewResult = filterContext.Result as ViewResultBase;
            if (viewResult == null)
                return;

            var baseViewModel = viewResult.ViewData.Model as BaseViewModel;
            if (baseViewModel == null)
                return;

            if (baseViewModel.CurrentUser == null)
                baseViewModel.CurrentUser = _authenticationService.Authenticated();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }
    }
}
