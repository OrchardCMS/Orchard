using System;
using System.Web.Mvc;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewModels;
using Orchard.UI.PageTitle;
using Orchard.UI.Resources;
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

        public static void AddTitleParts(this HtmlHelper html, params string[] titleParts) {
            html.Resolve<IPageTitleBuilder>().AddTitleParts(titleParts);
        }

        public static MvcHtmlString Title(this HtmlHelper html, params string[] titleParts) {
            IPageTitleBuilder pageTitleBuilder = html.Resolve<IPageTitleBuilder>();

            pageTitleBuilder.AddTitleParts(titleParts);

            return MvcHtmlString.Create(pageTitleBuilder.GenerateTitle());
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, string partitions) where TModel : BaseViewModel {
            IZoneManager manager = html.Resolve<IZoneManager>();

            manager.Render(html, html.ViewData.Model.Zones, zoneName, partitions);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : BaseViewModel {
            html.Zone(zoneName, string.Empty);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, Action action) where TModel : BaseViewModel {
            //TODO: again, IoC could move this AddAction (or similar) method out of the data-bearing object
            html.ViewData.Model.Zones.AddAction(zoneName, x => action());
            html.Zone(zoneName, string.Empty);
        }

        public static void ZoneBody<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : BaseViewModel {
            html.Zone(zoneName, () => html.RenderBody());
        }

        public static void RegisterStyle(this HtmlHelper html, string fileName) {
            html.Resolve<IResourceManager>().RegisterStyle(fileName);
        }

        public static void RegisterScript(this HtmlHelper html, string fileName) {
            html.Resolve<IResourceManager>().RegisterHeadScript(fileName);
        }
    }
}