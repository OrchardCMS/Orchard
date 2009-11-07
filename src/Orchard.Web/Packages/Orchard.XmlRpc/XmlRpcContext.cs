using Orchard.XmlRpc.Models;

namespace Orchard.XmlRpc {
    public class XmlRpcContext {
        public XRpcMethodCall Request { get; set; }
        public XRpcMethodResponse Response { get; set; }
    }
}