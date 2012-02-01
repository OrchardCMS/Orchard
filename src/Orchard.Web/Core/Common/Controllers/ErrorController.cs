using System.Net;
using System.Web.Mvc;
using Orchard.Mvc;
using Orchard.Themes;

namespace Orchard.Core.Common.Controllers {
    [Themed]
    public class ErrorController : Controller {
        private readonly IOrchardServices _orchardServices;

        public ErrorController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public ActionResult NotFound(string url) {
            Response.StatusCode = (int)HttpStatusCode.NotFound;
            var model = _orchardServices.New.NotFound();

            if(url == null) {
                url = Request.Url.OriginalString;
            }

            // If the url is relative then replace with Requested path
            model.RequestedUrl = Request.Url.OriginalString.Contains(url) & Request.Url.OriginalString != url ?
                Request.Url.OriginalString : url;
            
            // Dont get the user stuck in a 'retry loop' by
            // allowing the Referrer to be the same as the Request
            model.ReferrerUrl = Request.UrlReferrer != null &&
                Request.UrlReferrer.OriginalString != model.RequestedUrl ?
                Request.UrlReferrer.OriginalString : null;

            return new ShapeResult(this, model);
        }
    }
}