using System.Web;
using Orchard.XmlRpc.Models;

namespace Orchard.XmlRpc {
    public class XmlRpcContext {
        public HttpContextBase HttpContext { get; set; }
        public XRpcMethodCall Request { get; set; }
        public XRpcMethodResponse Response { get; set; }
    }
}