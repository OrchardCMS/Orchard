using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.Core.XmlRpc {
    public class XmlRpcContext {
        public ControllerContext ControllerContext { get; set; } 
        public HttpContextBase HttpContext { get; set; }
        public XRpcMethodCall Request { get; set; }
        public XRpcMethodResponse Response { get; set; }
        public ICollection<IXmlRpcDriver> _drivers = new List<IXmlRpcDriver>();
    }
}