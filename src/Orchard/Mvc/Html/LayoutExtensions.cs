using System;
using System.Web.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewModels;
using Orchard.UI.Zones;

namespace Orchard.Mvc.Html {
    public static class LayoutExtensions {
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

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, string partitions) where TModel : BaseViewModel {
            //is IoC necessary for this to work properly?
            var manager = new ZoneManager();
            manager.Render(html, html.ViewData.Model.Zones, zoneName, partitions);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : BaseViewModel {
            Zone(html, zoneName, string.Empty);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, Action action) where TModel : BaseViewModel {
            html.ViewData.Model.Zones.AddAction(zoneName, x => action());
            Zone(html, zoneName, string.Empty);
        }

        public static void ZoneBody<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : BaseViewModel {
            html.Zone(zoneName, () => html.RenderBody());
        }

        public static void RegisterStyle(this HtmlHelper html, string styleName) {
            //todo: register the style
        }
    }
}