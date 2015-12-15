using System.Xml.Linq;

namespace Orchard.Core.XmlRpc {
    public interface IXmlRpcHandler : IDependency {
        void SetCapabilities(XElement element);
        void Process(XmlRpcContext context);
    }
}