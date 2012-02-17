using System.Web.Routing;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Routing {
    public interface IArchiveConstraint : IRouteConstraint, ISingletonDependency {
        string FindPath(string path);
        ArchiveData FindArchiveData(string path);
    }
}