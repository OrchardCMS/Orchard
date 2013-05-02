using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.Controllers {
    public class WorkflowController : Controller {
        private readonly IWorkflowManager _workflowManager;

        public WorkflowController(IWorkflowManager workflowManager) {
            _workflowManager = workflowManager;
        }

        // This could be invoked by external applications and services to trigger an event within the workflows.
        public ActionResult Callback() {

            // Right now, all workflow instances that are at the WebRequest activity node would continue as soon as a request
            // to this action comes in, but that should not happen; it should be controlled by activity configuration and evaluations.
            _workflowManager.TriggerEvent("WebRequest", null, () => {
                var dictionary = new Dictionary<string, object>();

                // Let's include query string stuff, so that the WebRequest activity can
                // potentially match against certain parameters to decide whether or not it should execute,
                // based on its configuration (yet to be defined).
                Request.QueryString.CopyTo(dictionary);

                return dictionary;
            });

            // A Redirect may have been set by one of the rule events.
            if (!string.IsNullOrEmpty(HttpContext.Response.RedirectLocation))
                return new EmptyResult();

            return new EmptyResult(); // Or maybe an "OK" string. It shouldn't really matter, just as long as we return HTTP 200 OK.
        }
    }
}