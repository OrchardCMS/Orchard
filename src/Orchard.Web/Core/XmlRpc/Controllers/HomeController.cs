using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Core.XmlRpc.Models;
using Orchard.Core.XmlRpc.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.Core.XmlRpc.Controllers {
    public class HomeController : Controller {
        private readonly IXmlRpcWriter _writer;
        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;
        private readonly IClock _clock;

        public HomeController(
            IXmlRpcWriter writer,
            IEnumerable<IXmlRpcHandler> xmlRpcHandlers,
            IClock clock) {
            _writer = writer;
            _xmlRpcHandlers = xmlRpcHandlers;
            _clock = clock;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpPost, ActionName("Index")]
        [AlwaysAccessible]
        public ActionResult ServiceEndpoint(XRpcMethodCall methodCall) {
            Logger.Debug("XmlRpc methodName {0}", methodCall.MethodName);
            var methodResponse = Dispatch(methodCall);

            if (methodResponse == null)
                throw new HttpException(500, "TODO: xmlrpc fault");

            var result = _writer.MapMethodResponse(methodResponse);
            return Content(result.ToString(), "text/xml");        
        }

        [HttpHead, ActionName("Index")]
        [AlwaysAccessible]
        public ActionResult ExistenceDiscovery() {
            //see http://blogs.msdn.com/b/vsofficedeveloper/archive/2008/03/11/office-existence-discovery-protocol.aspx
            Logger.Debug("Microsoft Office Existence Discovery");

            Response.AddHeader("Last-Modified", _clock.UtcNow.ToString("o"));
            return Content("", "text/xml");
        }

        private XRpcMethodResponse Dispatch(XRpcMethodCall request) {
            var context = new XmlRpcContext { ControllerContext = ControllerContext, HttpContext = HttpContext, Request = request };
            try {
                foreach (var handler in _xmlRpcHandlers) {
                    handler.Process(context);
                }
            }
            catch (OrchardCoreException e) {
                // if a core exception is raised, report the error message, otherwise signal a 500
                context.Response =  context.Response ?? new XRpcMethodResponse();
                context.Response.Fault = new XRpcFault(0, e.LocalizedMessage.ToString());
            }

            return context.Response;
        }
    }
}