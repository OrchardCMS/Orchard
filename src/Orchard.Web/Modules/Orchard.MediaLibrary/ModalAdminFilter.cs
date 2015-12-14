using System.Web.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;

namespace Orchard.MediaLibrary {
    public class ModalAdminFilter : FilterProvider, IResultFilter {
        private readonly IResourceManager _resourceManager;

        public ModalAdminFilter(IResourceManager resourceManager) {
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            // should only run on a full view rendering result
            if (!(filterContext.Result is ViewResult) || !UI.Admin.AdminFilter.IsApplied(filterContext.RequestContext))
                return;
            _resourceManager.Include("stylesheet", "~/Modules/Orchard.MediaLibrary/Styles/dialog-mode.css", null);
            _resourceManager.Include("script", "~/Modules/Orchard.MediaLibrary/Scripts/modal-window.js", null).AtFoot();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}