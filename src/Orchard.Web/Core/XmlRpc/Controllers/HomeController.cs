using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.XmlRpc.Models;
using Orchard.Core.XmlRpc.Services;
using Orchard.Logging;

namespace Orchard.Core.XmlRpc.Controllers {
    public class HomeController : Controller {
        private readonly IXmlRpcWriter _writer;
        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;

        public HomeController(
            IXmlRpcWriter writer,
            IEnumerable<IXmlRpcHandler> xmlRpcHandlers) {
            _writer = writer;
            _xmlRpcHandlers = xmlRpcHandlers;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        [HttpPost, ActionName("Index")]
        public ActionResult ServiceEndpoint(XRpcMethodCall methodCall) {
            Logger.Debug("XmlRpc methodName {0}", methodCall.MethodName);
            var methodResponse = Dispatch(methodCall);

            if (methodResponse == null)
                throw new HttpException(500, "TODO: xmlrpc fault");

            var content = _writer.MapMethodResponse(methodResponse).ToString();
            return Content(content, "text/xml");
        }

        private XRpcMethodResponse Dispatch(XRpcMethodCall request) {
            var context = new XmlRpcContext { ControllerContext = ControllerContext, HttpContext = HttpContext, Request = request };
            foreach (var handler in _xmlRpcHandlers)
                handler.Process(context);
            return context.Response;
        }
    }
}