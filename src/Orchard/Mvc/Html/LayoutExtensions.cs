using System;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using Orchard.Mvc.ViewEngines;
using Orchard.Mvc.ViewModels;
using Orchard.UI.PageClass;
using Orchard.UI.PageTitle;
using Orchard.UI.Resources;
using Orchard.UI.Zones;

namespace Orchard.Mvc.Html {
    public static class LayoutExtensions {
        public static void RenderBody(this HtmlHelper html) {
            LayoutViewContext layoutViewContext = LayoutViewContext.From(html.ViewContext);
            html.ViewContext.Writer.Write(layoutViewContext.BodyContent);
        }

        public static MvcHtmlString Body(this HtmlHelper html) {
            LayoutViewContext layoutViewContext = LayoutViewContext.From(html.ViewContext);

            return MvcHtmlString.Create(layoutViewContext.BodyContent);
        }

        public static void AddTitleParts(this HtmlHelper html, params string[] titleParts) {
            html.Resolve<IPageTitleBuilder>().AddTitleParts(titleParts);
        }

        public static void AppendTitleParts(this HtmlHelper html, params string[] titleParts) {
            html.Resolve<IPageTitleBuilder>().AppendTitleParts(titleParts);
        }

        public static MvcHtmlString Title(this HtmlHelper html, params string[] titleParts) {
            IPageTitleBuilder pageTitleBuilder = html.Resolve<IPageTitleBuilder>();

            html.AppendTitleParts(titleParts);

            return MvcHtmlString.Create(html.Encode(pageTitleBuilder.GenerateTitle()));
        }

        public static MvcHtmlString TitleForPage(this HtmlHelper html, params string[] titleParts) {
            if (titleParts == null || titleParts.Length < 1)
                return null;

            html.AppendTitleParts(titleParts);

            return MvcHtmlString.Create(html.Encode(titleParts[0]));
        }

        public static void AddPageClassNames(this HtmlHelper html, params object[] classNames) {
            html.Resolve<IPageClassBuilder>().AddClassNames(classNames);
        }

        public static MvcHtmlString ClassForPage(this HtmlHelper html, params object[] classNames) {
            IPageClassBuilder pageClassBuilder = html.Resolve<IPageClassBuilder>();

            html.AddPageClassNames(classNames);
            //todo: (heskew) need ContentItem.ContentType
            html.AddPageClassNames(html.ViewContext.RouteData.Values["area"]);

            return MvcHtmlString.Create(html.Encode(pageClassBuilder.ToString()));
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, string partitions) where TModel : IZoneContainer {
            var manager = html.Resolve<IZoneManager>();

            manager.Render(html, html.ViewData.Model.Zones, zoneName, partitions, null);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel : IZoneContainer {
            html.Zone(zoneName, string.Empty);
        }

        public static void Zone<TModel>(this HtmlHelper<TModel> html, string zoneName, Action action) where TModel : IZoneContainer {
            //TODO: again, IoC could move this AddAction (or similar) method out of the data-bearing object
            html.ViewData.Model.Zones.AddAction(zoneName, x => action());
            html.Zone(zoneName, string.Empty);
        }

        public static void ZonesAny<TModel>(this HtmlHelper<TModel> html) where TModel : IZoneContainer {
            html.ZonesExcept();
        }

        public static void ZonesExcept<TModel>(this HtmlHelper<TModel> html, params string[] except) where TModel : IZoneContainer {
            var manager = html.Resolve<IZoneManager>();
            manager.Render(html, html.ViewData.Model.Zones, null, null, except);
        }

        //public static void ZoneBody<TModel>(this HtmlHelper<TModel> html, string zoneName) where TModel  {
        //    html.Zone(zoneName, () => html.RenderBody());
        //}

        public static void RegisterMeta(this HtmlHelper html, string name, string content) {
            html.Resolve<IResourceManager>().RegisterMeta(name, content);
        }

        public static void RegisterLink(this HtmlHelper html, LinkEntry entry) {
            html.Resolve<IResourceManager>().RegisterLink(entry, html);
        }

        public static FileRegistrationContext RegisterStyle(this HtmlHelper html, string fileName) {
            return html.Resolve<IResourceManager>().RegisterStyle(fileName, html);
        }

        public static FileRegistrationContext RegisterStyle(this HtmlHelper html, string fileName, string position) {
            return html.Resolve<IResourceManager>().RegisterStyle(fileName, html, position);
        }

        public static FileRegistrationContext RegisterScript(this HtmlHelper html, string fileName) {
            return html.Resolve<IResourceManager>().RegisterHeadScript(fileName, html);
        }

        public static FileRegistrationContext RegisterScript(this HtmlHelper html, string fileName, string position) {
            return html.Resolve<IResourceManager>().RegisterHeadScript(fileName, html, position);
        }

        public static FileRegistrationContext RegisterFootScript(this HtmlHelper html, string fileName) {
            return html.Resolve<IResourceManager>().RegisterFootScript(fileName, html);
        }

        public static FileRegistrationContext RegisterFootScript(this HtmlHelper html, string fileName, string position) {
            return html.Resolve<IResourceManager>().RegisterFootScript(fileName, html, position);
        }

        public static IDisposable Capture(this ViewUserControl control, string name) {
            var writer = LayoutViewContext.From(control.ViewContext).GetNamedContent(name);
            return new HtmlTextWriterScope(control.Writer, writer);
        }

        class HtmlTextWriterScope : IDisposable {
            private readonly HtmlTextWriter _context;
            private readonly TextWriter _writer;

            public HtmlTextWriterScope(HtmlTextWriter context, TextWriter writer) {
                _context = context;
                _writer = _context.InnerWriter;
                _context.InnerWriter = writer;
            }

            public void Dispose() {
                _context.InnerWriter = _writer;
            }
        }

    }
}