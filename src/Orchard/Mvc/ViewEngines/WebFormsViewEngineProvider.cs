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
                AreaPartialViewLocationFormats=DisabledFormats,
            };

            // enable /Views/{partialName}
            // enable /Views/"DisplayTemplates/"+{templateName}
            // enable /Views/"EditorTemplates/+{templateName}
            viewEngine.PartialViewLocationFormats = new[] {
                parameters.VirtualPath + "/Views/{0}.ascx",
                parameters.VirtualPath + "/Views/{0}.aspx",
            };

            // for "routed" request views...
            // enable /Views/{area}/{controller}/{viewName}
            viewEngine.AreaPartialViewLocationFormats = new[] {
                parameters.VirtualPath + "/Views/{2}/{1}/{0}.ascx",
                parameters.VirtualPath + "/Views/{2}/{1}/{0}.aspx",
            };

            return viewEngine;
        }

        public IViewEngine CreatePackagesViewEngine(CreatePackagesViewEngineParams parameters) {
            var areaFormats = new[] {
                                        "~/Core/{2}/Views/{1}/{0}.ascx",
                                        "~/Core/{2}/Views/{1}/{0}.aspx",
                                        "~/Packages/{2}/Views/{1}/{0}.ascx",
                                        "~/Packages/{2}/Views/{1}/{0}.aspx",
                                    };

            var universalFormats = parameters.VirtualPaths
                .SelectMany(x => new[] {
                                           x + "/Views/{0}.ascx",
                                           x + "/Views/{0}.aspx",
                                       })
                .ToArray();

            var viewEngine = new WebFormViewEngine {
                MasterLocationFormats = DisabledFormats,
                ViewLocationFormats = universalFormats,
                PartialViewLocationFormats = universalFormats,
                AreaMasterLocationFormats = DisabledFormats,
                AreaViewLocationFormats = areaFormats,
                AreaPartialViewLocationFormats = areaFormats,
            };

            return viewEngine;
        }
    }
}
