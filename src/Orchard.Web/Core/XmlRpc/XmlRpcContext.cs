using System.Web;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.Core.XmlRpc {
    public class XmlRpcContext {
        public HttpContextBase HttpContext { get; set; }
        public XRpcMethodCall Request { get; set; }
        public XRpcMethodResponse Response { get; set; }
    }
}