using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Orchard.WebApi.Filters {
    public class OrchardApiActionFilterDispatcher : IActionFilter {
        public bool AllowMultiple { get; private set; }
        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation) {
            var workContext = actionContext.ControllerContext.GetWorkContext();

            foreach (var actionFilter in workContext.Resolve<IEnumerable<IApiFilterProvider>>().OfType<IActionFilter>()) {
                var tempContinuation = continuation;
                continuation = () => {
                    return actionFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, tempContinuation);
                };
            }

            return await continuation();
        }
    }
}