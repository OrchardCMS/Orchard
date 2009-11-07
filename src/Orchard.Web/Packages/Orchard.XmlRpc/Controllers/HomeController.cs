using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Orchard.Logging;
using Orchard.XmlRpc.Models;

namespace Orchard.XmlRpc.Controllers {

    public class HomeController : Controller {
        private readonly IMapper<XRpcMethodResponse, XElement> _writer;
        private readonly IEnumerable<IXmlRpcHandler> _xmlRpcHandlers;

        public HomeController(
            IMapper<XRpcMethodResponse, XElement> writer,
            IEnumerable<IXmlRpcHandler> xmlRpcHandlers) {
            _writer = writer;
            _xmlRpcHandlers = xmlRpcHandlers;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ActionResult Index() {
            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult ServiceEndpoint(XRpcMethodCall methodCall) {
            Logger.Debug("XmlRpc methodName {0}", methodCall.MethodName);
            var methodResponse = Dispatch(methodCall);

            if (methodResponse == null)
                throw new HttpException(500, "TODO: xmlrpc fault");

            var content = _writer.Map(methodResponse).ToString();
            return Content(content, "text/xml");
        }

        private XRpcMethodResponse Dispatch(XRpcMethodCall request) {
            var context = new XmlRpcContext { Request = request };
            foreach (var handler in _xmlRpcHandlers)
                handler.Process(context);
            return context.Response;
        }
    }
}
