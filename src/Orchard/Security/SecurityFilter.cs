using System;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.Security {
    [UsedImplicitly]
    public class SecurityFilter : FilterProvider, IResultFilter, IExceptionFilter {
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
                baseViewModel.CurrentUser = _authenticationService.GetAuthenticatedUser();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {

        }

        public void OnException(ExceptionContext filterContext) {
            if (filterContext.Exception is UnauthorizedException) {
                filterContext.Result = new HttpUnauthorizedResult();
                filterContext.ExceptionHandled = true;
            }
        }
    }
}
