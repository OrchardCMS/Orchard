using System.Web.Hosting;

namespace Orchard.Environment {
    public interface ICustomVirtualPathProvider {
        VirtualPathProvider Instance { get; }
    }
}