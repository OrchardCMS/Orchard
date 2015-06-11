using System;
using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;

namespace Orchard.MediaPicker {
    public class AdminFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;

        public AdminFilter(IResourceManager resourceManager) {
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult) || !Orchard.UI.Admin.AdminFilter.IsApplied(filterContext.RequestContext))
                return;
            _resourceManager.Require("script", "jQuery");
            _resourceManager.Include("script", "~/Modules/Orchard.MediaPicker/Scripts/MediaPicker.js", null);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}