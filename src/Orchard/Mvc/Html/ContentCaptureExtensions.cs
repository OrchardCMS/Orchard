using System.Web.Mvc;
using Orchard.UI.ContentCapture;

namespace Orchard.Mvc.Html {
    public static class ContentCaptureExtensions {
        public static ContentCaptureBlock CaptureContent(this HtmlHelper htmlHelper, string captureName) {
            IContentCapture contentCapture = htmlHelper.Resolve<IContentCapture>();

            //htmlHelper.ViewContext.HttpContext.Response.Filter.Flush();
            contentCapture.BeginContentCapture(captureName);

            return new ContentCaptureBlock(htmlHelper.ViewContext, contentCapture);
        }
    }
}