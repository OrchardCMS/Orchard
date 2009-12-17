using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewModels;

namespace Orchard.UI.Resources {
    public class ResourceFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;

        public ResourceFilter(IResourceManager resourceManager) {
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            BaseViewModel model = filterContext.Controller.ViewData.Model as BaseViewModel;

            if (model != null) {
                model.Zones.AddAction("head:metas", html => html.ViewContext.HttpContext.Response.Output.Write(_resourceManager.GetMetas()));
                model.Zones.AddAction("head:styles", html => html.ViewContext.HttpContext.Response.Output.Write(_resourceManager.GetStyles(filterContext.RequestContext)));
                model.Zones.AddAction("head:scripts", html => html.ViewContext.HttpContext.Response.Output.Write(_resourceManager.GetHeadScripts(filterContext.RequestContext)));
                model.Zones.AddAction("body:after", html => html.ViewContext.HttpContext.Response.Output.Write(_resourceManager.GetFootScripts(filterContext.RequestContext)));
            }
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}