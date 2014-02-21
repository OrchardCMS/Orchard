using System;
using System.Web;
using System.Web.Routing;
using Orchard.Autoroute.Services;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;

namespace Orchard.Blogs.Routing {
    public class ArchiveConstraint : IArchiveConstraint {
        private readonly IPathResolutionService _pathResolutionService;

        public ArchiveConstraint(IPathResolutionService pathResolutionService) {
            _pathResolutionService = pathResolutionService;
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

                var autoroute = _pathResolutionService.GetPath(path);

                return autoroute != null && autoroute.Is<BlogPart>();
            }

            return false;
        }

        public string FindPath(string path) {
            // my-blog/archive
            if (path.EndsWith("/archive")) {
                return path.Substring(0, path.Length - "/archive".Length);
            }

            // my-blog/archive/
            if (path.EndsWith("/archive/")) {
                return null;
            }

            // my-blog/archive/2014
            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {

                // archive/
                if (path.EndsWith("archive/")) {
                    return null;
                }

                // archive
                if (path.EndsWith("archive")) {
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
            if (path.EndsWith("/archive")) {
                return new ArchiveData("");
            }

            // my-blog/archive/
            if (path.EndsWith("/archive/")) {
                return null;
            }

            var archiveIndex = path.IndexOf("/archive/", StringComparison.OrdinalIgnoreCase);

            if (archiveIndex == -1) {

                // archive/
                if (path.EndsWith("archive/")) {
                    return null;
                }

                // archive
                if (path.EndsWith("archive")) {
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