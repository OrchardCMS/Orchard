using System.Collections.Generic;

namespace Orchard.Core.XmlRpc.Models {
    public class XRpcMethodCall {
        public XRpcMethodCall() { Params = new List<XRpcData>(); }

        public string MethodName { get; set; }
        public IList<XRpcData> Params { get; set; }
    }
}