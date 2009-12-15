using System.Linq;
using System.Web.Mvc;

namespace Orchard.Mvc.ViewEngines {
    public class WebFormsViewEngineProvider : IViewEngineProvider {

        static string[] DisabledFormats = new[] { "~/Disabled" };

        public IViewEngine CreateThemeViewEngine(CreateThemeViewEngineParams parameters) {
            // Area: if "area" in RouteData. Url hit for package...
            // Area-Layout Paths - no-op because LayoutViewEngine uses multi-pass instead of layout paths
            // Area-View Paths - no-op because LayoutViewEngine relies entirely on Partial view resolution
            // Area-Partial Paths - enable theming views associated with a package based on the route

            // Layout Paths - no-op because LayoutViewEngine uses multi-pass instead of layout paths
            // View Paths - no-op because LayoutViewEngine relies entirely on Partial view resolution
            // Partial Paths - 
            //   {area}/{controller}/


            var viewEngine = new WebFormViewEngine {
                MasterLocationFormats = DisabledFormats,
                ViewLocationFormats = DisabledFormats,
                AreaMasterLocationFormats = DisabledFormats,
                AreaViewLocationFormats = DisabledFormats,
            };

            // enable /Views/Shared/{partialName}
            // enable /Views/Shared/"DisplayTemplates/"+{templateName}
            // enable /Views/Shared/"EditorTemplates/+{templateName}
            viewEngine.PartialViewLocationFormats = new[] {
                parameters.VirtualPath + "/Views/Shared/{0}.ascx",
            };

            // for "routed" request views...
            // enable /Views/{area}/{controller}/{viewName}
            viewEngine.AreaPartialViewLocationFormats = new[] {
                parameters.VirtualPath + "/Views/{2}/{1}/{0}.ascx",
            };

            return viewEngine;
        }

        public IViewEngine CreatePackagesViewEngine(CreatePackagesViewEngineParams parameters) {
            var viewEngine = new WebFormViewEngine {
                MasterLocationFormats = DisabledFormats,
                ViewLocationFormats = DisabledFormats,
                AreaMasterLocationFormats = DisabledFormats,
                AreaViewLocationFormats = DisabledFormats,
                AreaPartialViewLocationFormats = DisabledFormats,
            };

            viewEngine.PartialViewLocationFormats = parameters.VirtualPaths
                .Select(x => x + "/Views/Shared/{0}.ascx")
                .ToArray();

            return viewEngine;
        }
    }
}
