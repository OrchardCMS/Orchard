using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Orchard.Localization;
using Orchard.Mvc.Filters;
using Orchard.WebApi.Filters;

namespace Orchard.Blogs.Controllers {
    public class HelloController : ApiController {

        public HelloController(
            IOrchardServices orchardServices) {
            Services = orchardServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public string Get() {
            return "Hello, world!";
        }
    }

    public class MyFilter : ActionFilterAttribute, IApiFilterProvider {
        public override void OnActionExecuting(HttpActionContext actionContext) {
            actionContext.Response.StatusCode = HttpStatusCode.NoContent;
        }
    }
}