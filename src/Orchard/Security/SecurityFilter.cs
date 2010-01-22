using System;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.Security {
    [UsedImplicitly]
    public class SecurityFilter : FilterProvider, IResultFilter, IExceptionFilter {
        private readonly IAuthenticationService _authenticationService;

        public SecurityFilter(IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

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
            if (!(filterContext.Exception is OrchardSecurityException)) {
                return;
            }

            try {
                Logger.Information(filterContext.Exception, "Security exception converted to access denied result");
            }
            catch {
                //a logger exception can't be allowed to interrupt this process
            }

            filterContext.Result = new HttpUnauthorizedResult();
            filterContext.ExceptionHandled = true;
        }
    }
}
