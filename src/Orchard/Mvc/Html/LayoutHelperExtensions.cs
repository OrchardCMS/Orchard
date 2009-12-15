using System.Web.Mvc;
using Orchard.Mvc.ViewEngines;

namespace Orchard.Mvc.Html {
    public static class LayoutHelperExtensions {
        public static void RenderBody(this HtmlHelper html) {
            var layoutContext = OrchardLayoutContext.From(html.ViewContext);
            html.ViewContext.HttpContext.Response.Output.Write(layoutContext.BodyContent);
        }
    }
}
