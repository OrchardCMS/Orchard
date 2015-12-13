using System.Web.Hosting;

namespace Orchard.Environment.Extensions.Helpers {
    public static class PathHelpers {
        public static string GetPhysicalPath(string path) {
            if (path.StartsWith("~") && HostingEnvironment.IsHosted) {
                return HostingEnvironment.MapPath(path);
            }
            return path;
        }
    }
}