using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Orchard.Mvc.Html {
    public class MvcFormAntiForgeryPost : MvcForm {
        private readonly HtmlHelper _htmlHelper;

        public MvcFormAntiForgeryPost(HtmlHelper htmlHelper) : base(htmlHelper.ViewContext) {
            _htmlHelper = htmlHelper;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (!_htmlHelper.ViewContext.HttpContext.Response.HeadersWritten) {
                    _htmlHelper.ViewContext.Writer.Write(_htmlHelper.AntiForgeryTokenOrchard());
                }
            }

            base.Dispose(disposing);
        }
    }
}
