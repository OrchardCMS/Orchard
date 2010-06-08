using System.Collections.Generic;
using System.Web.Routing;

namespace Orchard.Core.Routable {
    public interface IRoutablePathConstraint : IRouteConstraint, ISingletonDependency {
        void SetPaths(IEnumerable<string> paths);
        string FindPath(string path);
        void AddPath(string path);
        void RemovePath(string path);
    }
}
