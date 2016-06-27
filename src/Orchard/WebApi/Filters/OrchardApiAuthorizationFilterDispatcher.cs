using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Orchard.WebApi.Filters {
    public class OrchardApiAuthorizationFilterDispatcher : IAuthorizationFilter {
        public bool AllowMultiple { get; private set; }
        public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation) {
            var workContext = actionContext.ControllerContext.GetWorkContext();

            foreach (var actionFilter in workContext.Resolve<IEnumerable<IApiFilterProvider>>().OfType<IAuthorizationFilter>()) {
                var tempContinuation = continuation;
                continuation = () => {
                    return actionFilter.ExecuteAuthorizationFilterAsync(actionContext, cancellationToken, tempContinuation);
                };
            }

            return await continuation();
        }
    }
}