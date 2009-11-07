using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.XmlRpc {
    public interface IXmlRpcHandler : IDependency {
        void Process(XmlRpcContext context);
    }
}
