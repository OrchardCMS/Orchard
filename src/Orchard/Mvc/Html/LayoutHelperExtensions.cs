using System.Web.Mvc;
using Orchard.Mvc.ViewEngines;

namespace Orchard.Mvc.Html {
    public static class LayoutHelperExtensions {
        public static void RenderBody(this HtmlHelper html) {
            OrchardLayoutContext layoutContext = OrchardLayoutContext.From(html.ViewContext);
            html.ViewContext.HttpContext.Response.Output.Write(layoutContext.BodyContent);
        }

        public static MvcHtmlString Body(this HtmlHelper html) {
            OrchardLayoutContext layoutContext = OrchardLayoutContext.From(html.ViewContext);
            return MvcHtmlString.Create(layoutContext.BodyContent);
        }

        public static void RenderTitle(this HtmlHelper html) {
            OrchardLayoutContext layoutContext = OrchardLayoutContext.From(html.ViewContext);
            html.ViewContext.HttpContext.Response.Output.Write(layoutContext.Title);
        }

        public static MvcHtmlString Title(this HtmlHelper html) {
            OrchardLayoutContext layoutContext = OrchardLayoutContext.From(html.ViewContext);
            return MvcHtmlString.Create(html.Encode(layoutContext.Title));
        }

        public static void RenderZone(this HtmlHelper html, string zoneName) {
            OrchardLayoutContext layoutContext = OrchardLayoutContext.From(html.ViewContext);
            html.ViewContext.HttpContext.Response.Output.Write(layoutContext.Title);
        }

        public static void RegisterStyle(this HtmlHelper html, string styleName) {
            //todo: register the style
        }
    }
}