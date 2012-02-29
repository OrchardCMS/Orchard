using System;
using System.Web;
using System.Web.Routing;
using Orchard.Blogs.Models;

namespace Orchard.Blogs.Routing {
    public class ArchiveConstraint : IArchiveConstraint {
        private readonly IBlogPathConstraint _blogPathConstraint;

        public ArchiveConstraint(IBlogPathConstraint blogPathConstraint) {
            _blogPathConstraint = blogPathConstraint;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                var path = FindPath(parameterValue);
                if (path == null) {
                    return false;
                }

                var archiveData = FindArchiveData(parameterValue);
                if (archiveData == null) {
                    return false;
                }

                return _blogPathConstraint.FindPath(path) != null;
            }

            return false;
        }

        public string FindPath(string path) {
            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {
                
                // archive for blog as homepage ?
                if(path.StartsWith("archive/", StringComparison.OrdinalIgnoreCase)) {
                    return String.Empty;
                }

                return null;
            }

            return path.Substring(0, archiveIndex);
        }

        public ArchiveData FindArchiveData(string path) {
            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {
                
                // archive for blog as homepage ?
                if (path.StartsWith("archive/", StringComparison.OrdinalIgnoreCase)) {
                    return new ArchiveData(path.Substring("archive/".Length));
                }

                return null;
            }

            try {
                return new ArchiveData(path.Substring(archiveIndex + "/archive/".Length));
            }
            catch {
                return null;
            }
        }
    }
}