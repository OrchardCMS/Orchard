using System.Web.Hosting;

namespace Orchard.FileSystems.VirtualPath {
    public interface ICustomVirtualPathProvider {
        VirtualPathProvider Instance { get; }
    }
}