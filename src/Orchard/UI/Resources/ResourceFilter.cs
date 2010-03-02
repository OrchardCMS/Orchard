using System.IO;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;

        public ResourceFilter(IResourceManager resourceManager) {
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            var model = BaseViewModel.From(filterContext.Result);
            if (model == null) {
                return;
            }

            model.Zones.AddAction("head:metas", html => html.ViewContext.Writer.Write(_resourceManager.GetMetas()));
            model.Zones.AddAction("head:styles", html => html.ViewContext.Writer.Write(_resourceManager.GetStyles()));
            model.Zones.AddAction("head:scripts", html => html.ViewContext.Writer.Write(_resourceManager.GetHeadScripts()));
            model.Zones.AddAction("body:after", html => {
                html.ViewContext.Writer.Write(_resourceManager.GetFootScripts());
                TextWriter captured;
                if (LayoutViewContext.From(html.ViewContext).Contents.TryGetValue("end-of-page-scripts", out captured)) {
                    html.ViewContext.Writer.Write(captured);
                }
            });
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}