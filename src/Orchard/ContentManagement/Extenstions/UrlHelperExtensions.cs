using System.Web.Mvc;

namespace Orchard.ContentManagement.Extenstions {
    public static class UrlHelperExtensions {
        public static string Slugify(this UrlHelper urlHelper) {
            return urlHelper.Action("Slugify", "Routable", new { area = "Common" });
        }
    }
}