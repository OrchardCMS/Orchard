using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Mvc.Filters;

namespace Orchard.Security {
    [UsedImplicitly]
    public class SecurityFilter : FilterProvider, IExceptionFilter {
        public SecurityFilter() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void OnException(ExceptionContext filterContext) {
            if (!(filterContext.Exception is OrchardSecurityException))
                return;

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
