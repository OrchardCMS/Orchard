using Orchard.Alias.Implementation.Holder;
using Orchard.Blogs.Models;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Orchard.Blogs.Routing {
    public class ArchiveConstraint : IArchiveConstraint {
        private readonly IAliasHolder _aliasHolder;

        public ArchiveConstraint(IAliasHolder aliasHolder) {
            _aliasHolder = aliasHolder;
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

                try {
                    // is this a valid date ?
                    archiveData.ToDateTime();
                }
                catch {
                    return false;
                }

                IDictionary<string, string> routeValues;
                if (!_aliasHolder.GetMap("Orchard.Blogs").TryGetAlias(path, out routeValues)) {
                    return false;
                }

                var isBlog = 
                    //routeValues.ContainsKey("area") &&
                    //routeValues["area"] == "Orchard.Blogs" &&
                    routeValues.ContainsKey("controller") &&
                    routeValues["controller"] == "Blog" &&
                    routeValues.ContainsKey("action") &&
                    routeValues["action"] == "Item" 
                    ;

                return isBlog;
            }

            return false;
        }

        public string FindPath(string path) {
            // my-blog/archive
            if (path.EndsWith("/archive", StringComparison.OrdinalIgnoreCase)) {
                return path.Substring(0, path.Length - "/archive".Length);
            }

            // my-blog/archive/
            if (path.EndsWith("/archive/", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            // my-blog/archive/2014
            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {

                // archive/
                if (path.EndsWith("archive/", StringComparison.OrdinalIgnoreCase)) {
                    return null;
                }

                // archive
                if (path.EndsWith("archive", StringComparison.OrdinalIgnoreCase)) {
                    return String.Empty;
                }

                // archive for blog as homepage ?
                if(path.StartsWith("archive/", StringComparison.OrdinalIgnoreCase)) {
                    return String.Empty;
                }

                return null;
            }

            return path.Substring(0, archiveIndex);
        }

        public ArchiveData FindArchiveData(string path) {
            // my-blog/archive
            if (path.EndsWith("/archive", StringComparison.OrdinalIgnoreCase)) {
                return new ArchiveData("");
            }

            // my-blog/archive/
            if (path.EndsWith("/archive/", StringComparison.OrdinalIgnoreCase)) {
                return null;
            }

            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {

                // archive/
                if (path.EndsWith("archive/", StringComparison.OrdinalIgnoreCase)) {
                    return null;
                }

                // archive
                if (path.EndsWith("archive", StringComparison.OrdinalIgnoreCase)) {
                    return new ArchiveData("");
                }

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