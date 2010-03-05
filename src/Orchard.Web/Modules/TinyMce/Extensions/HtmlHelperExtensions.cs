using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.ViewModels;
using Orchard.Extensions;
using Orchard.Mvc.Html;

namespace TinyMce.Extensions {
    public static class HtmlHelperExtensions {
        public static string GetCurrentMediaPath(this HtmlHelper<BodyEditorViewModel> htmlHelper) {
            var body = htmlHelper.ViewData.Model.BodyAspect;
            var currentDriver = htmlHelper.Resolve<IEnumerable<IContentItemDriver>>().Where(cid => cid.GetContentTypes().Any(ct => string.Compare(ct.Name, body.ContentItem.ContentType, true) == 0)).FirstOrDefault();
            var currentModule = htmlHelper.Resolve<IExtensionManager>().ActiveExtensions().FirstOrDefault(ee => ee.Descriptor.ExtensionType == "Module" && ee.Assembly == currentDriver.GetType().Assembly);
            var routable = body.ContentItem.Has<RoutableAspect>() ? body.ContentItem.As<RoutableAspect>() : null;

            //TODO: (erikpo) Get the value for the media folder from settings instead of hardcoding it
            return ((Control)htmlHelper.ViewDataContainer).ResolveUrl(string.Format("{0}/{1}{2}", "~/Media", currentModule.Descriptor.Name, routable != null && !string.IsNullOrEmpty(routable.ContainerPath) ? "/" + routable.ContainerPath : ""));
        }
    }
}