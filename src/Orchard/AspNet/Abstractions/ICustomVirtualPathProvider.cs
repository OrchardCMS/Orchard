using System.Web.Hosting;

namespace Orchard.AspNet.Abstractions {
    public interface ICustomVirtualPathProvider {
        VirtualPathProvider Instance { get; }
    }
}