using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.UI.ContentCapture;

namespace Orchard.Mvc.Html {
    public class ContentCaptureBlock : MvcForm {
        private readonly ViewContext _viewContext;
        private readonly IContentCapture _contentCapture;

        public ContentCaptureBlock(ViewContext viewContext, IContentCapture contentCapture)
            : base(viewContext) {
            _viewContext = viewContext;
            _contentCapture = contentCapture;
        }

        protected override void Dispose(bool disposing) {
            //_viewContext.HttpContext.Response.Filter.Flush();
            _contentCapture.EndContentCapture();
        }
    }
}