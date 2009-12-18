using System.IO;
using System.Web.Mvc;
using Orchard.Mvc.Filters;

namespace Orchard.UI.ContentCapture {
    public class ContentCaptureFilter : FilterProvider, IResultFilter {
        private readonly IContentCapture _contentCapture;

        public ContentCaptureFilter(IContentCapture contentCapture) {
            //_contentCapture = contentCapture;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext) {
            //if (filterContext.Result is ViewResult) {
            //    _contentCapture.CaptureStream = filterContext.HttpContext.Response.Filter;
            //    filterContext.HttpContext.Response.Filter = _contentCapture as Stream;
            //    filterContext.HttpContext.Response.Buffer = false;
            //}
        }

        public void OnResultExecuted(ResultExecutedContext filterContext) {
        }
    }
}