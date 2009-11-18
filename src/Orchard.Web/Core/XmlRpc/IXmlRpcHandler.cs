namespace Orchard.Core.XmlRpc {
    public interface IXmlRpcHandler : IDependency {
        void Process(XmlRpcContext context);
    }
}