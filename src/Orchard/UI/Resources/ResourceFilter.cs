using System.IO;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewEngines;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ResourceFilter(IResourceManager resourceManager, IWorkContextAccessor workContextAccessor) {
            _resourceManager = resourceManager;
            _workContextAccessor = workContextAccessor;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var headZone = _workContextAccessor.GetContext().CurrentPage.Zones["Head"];
            headZone.Add(html => html.ViewContext.Writer.Write(_resourceManager.GetMetas()), ":metas");
            headZone.Add(html => html.ViewContext.Writer.Write(_resourceManager.GetStyles()), ":styles");
            headZone.Add(html => html.ViewContext.Writer.Write(_resourceManager.GetLinks(html)), ":links");
            headZone.Add(html => html.ViewContext.Writer.Write(_resourceManager.GetHeadScripts()), ":scripts");

            _workContextAccessor.GetContext().CurrentPage.Zones["Body"].Add(
                html => {
                    html.ViewContext.Writer.Write(_resourceManager.GetFootScripts());
                    TextWriter captured;
                    if (LayoutViewContext.From(html.ViewContext).Contents.TryGetValue("end-of-page-scripts", out captured))
                        html.ViewContext.Writer.Write(captured);
                },
                ":after");
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {}
    }
}