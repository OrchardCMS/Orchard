using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.AspNet.Abstractions;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ResourceBindingStrategy;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI;
using Orchard.UI.Resources;
using Orchard.UI.Zones;
using Orchard.Utility.Extensions;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Shapes {
    public class CoreShapes : IShapeTableProvider {
        private readonly Work<WorkContext> _workContext;
        private readonly Work<IResourceManager> _resourceManager;
        private readonly Work<IHttpContextAccessor> _httpContextAccessor;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ITagBuilderFactory _tagBuilderFactory;

        public CoreShapes(
            ITagBuilderFactory tagBuilderFactory,
            Work<WorkContext> workContext, 
            Work<IResourceManager> resourceManager,
            Work<IHttpContextAccessor> httpContextAccessor,
            IVirtualPathProvider virtualPathProvider
            ) {
            _tagBuilderFactory = tagBuilderFactory;
            _workContext = workContext;
            _resourceManager = resourceManager;
            _httpContextAccessor = httpContextAccessor;
            _virtualPathProvider = virtualPathProvider;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe("Layout")
                .Configure(descriptor => descriptor.Wrappers.Add("Document"))
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(() => creating.New.Zone())))
                .OnCreated(created => {
                    var layout = created.Shape;
                    
                    layout.Head = created.New.DocumentZone(ZoneName: "Head");
                    layout.Body = created.New.DocumentZone(ZoneName: "Body");
                    layout.Tail = created.New.DocumentZone(ZoneName: "Tail");

                    layout.Body.Add(created.New.PlaceChildContent(Source: layout));

                    layout.Content = created.New.Zone();
                    layout.Content.ZoneName = "Content";
                    layout.Content.Add(created.New.PlaceChildContent(Source: layout));

                });

            // 'Zone' shapes are built on the Zone base class
            // They have class="zone zone-{name}"
            // and the template can be specialized with "Zone-{Name}" base file name
            builder.Describe("Zone")
                .OnCreating(creating => creating.BaseType = typeof(Zone))
                .OnDisplaying(displaying => {
                    var zone = displaying.Shape;
                    string zoneName = zone.ZoneName;
                    zone.Classes.Add("zone-" + zoneName.HtmlClassify());
                    zone.Classes.Add("zone");

                    // Zone__[ZoneName] e.g. Zone-SideBar
                    zone.Metadata.Alternates.Add("Zone__" + zoneName);
                });

            builder.Describe("Menu")
                .OnDisplaying(displaying => {
                    var menu = displaying.Shape;
                    string menuName = menu.MenuName;
                    menu.Classes.Add("menu-" + menuName.HtmlClassify());
                    menu.Classes.Add("menu");
                    menu.Metadata.Alternates.Add("Menu__" + menuName);
                });

            builder.Describe("MenuItem")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    menuItem.Metadata.Alternates.Add("MenuItem__" + menu.MenuName);
                });

            builder.Describe("LocalMenu")
                .OnDisplaying(displaying => {
                    var menu = displaying.Shape;
                    string menuName = menu.MenuName;
                    menu.Classes.Add("localmenu-" + menuName.HtmlClassify());
                    menu.Classes.Add("localmenu");
                    menu.Metadata.Alternates.Add("LocalMenu__" + menuName);
                });

            builder.Describe("LocalMenuItem")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    menuItem.Metadata.Alternates.Add("LocalMenuItem__" + menu.MenuName);
                });

            // 'List' shapes start with several empty collections
            builder.Describe("List")
                .OnCreated(created => {
                    var list = created.Shape;
                    list.Tag = "ul";
                    list.ItemClasses = new List<string>();
                    list.ItemAttributes = new Dictionary<string, string>();
                });

            builder.Describe("Style")
                .OnDisplaying(displaying => {
                    var resource = displaying.Shape;
                    string url = resource.Url;
                    string fileName = StylesheetBindingStrategy.GetAlternateShapeNameFromFileName(url);
                    if (!string.IsNullOrEmpty(fileName)) {
                        resource.Metadata.Alternates.Add("Style__" + fileName);
                    }
                });

            builder.Describe("Resource")
                .OnDisplaying(displaying => {
                    var resource = displaying.Shape;
                    string url = resource.Url;
                    string fileName = StylesheetBindingStrategy.GetAlternateShapeNameFromFileName(url);
                    if (!string.IsNullOrEmpty(fileName)) {
                        resource.Metadata.Alternates.Add("Resource__" + fileName);
                    }
                });
        }


        private static TagBuilder GetTagBuilder(string tagName, string id, object classes, object additionalClasses, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            // classes
            var enumerableClasses = classes as IEnumerable<string>;
            if (enumerableClasses == null && classes is string) {
                enumerableClasses = ((string)classes).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            foreach (var cssClass in enumerableClasses ?? Enumerable.Empty<string>()) {
                tagBuilder.AddCssClass(cssClass);
            }
            // additional classes
            enumerableClasses = additionalClasses as IEnumerable<string>;
            if (enumerableClasses == null && additionalClasses is string) {
                enumerableClasses = ((string)additionalClasses).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            foreach (var cssClass in enumerableClasses ?? Enumerable.Empty<string>()) {
                tagBuilder.AddCssClass(cssClass);
            }
            // id
            if (!string.IsNullOrWhiteSpace(id)) {
                tagBuilder.GenerateId(id);
            }
            return tagBuilder;
        }

        [Shape]
        public void Zone(dynamic Display, dynamic Shape, TextWriter Output) {
            string id = Shape.Id;
            IEnumerable<string> classes = Shape.Classes;
            IDictionary<string, string> attributes = Shape.Attributes;
            var zoneWrapper = GetTagBuilder("div", id, classes, null, attributes);
            Output.Write(zoneWrapper.ToString(TagRenderMode.StartTag));
            foreach (var item in ordered_hack(Shape))
                Output.Write(Display(item));
            Output.Write(zoneWrapper.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void ContentZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in ordered_hack(Shape))
                Output.Write(Display(item));
        }

        [Shape]
        public void DocumentZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in ordered_hack(Shape))
                Output.Write(Display(item));
        }

        #region ordered_hack

        private static IEnumerable<dynamic> ordered_hack(dynamic shape) {
            IEnumerable<dynamic> unordered = shape;
            if (unordered == null || unordered.Count() < 2)
                return shape;

            var i = 1;
            var progress = 1;
            var flatPositionComparer = new PositionComparer();
            var ordering = unordered.Select(item => {
                var position = (item == null || item.GetType().GetProperty("Metadata") == null || item.Metadata.GetType().GetProperty("Position") == null)
                                   ? null
                                   : item.Metadata.Position;
                return new { item, position };
            }).ToList();

            // since this isn't sticking around (hence, the "hack" in the name), throwing (in) a gnome 
            while (i < ordering.Count()) {
                if (flatPositionComparer.Compare(ordering[i].position, ordering[i - 1].position) > -1) {
                    if (i == progress)
                        progress = ++i;
                    else
                        i = progress;
                }
                else {
                    var higherThanItShouldBe = ordering[i];
                    ordering[i] = ordering[i - 1];
                    ordering[i - 1] = higherThanItShouldBe;
                    if (i > 1)
                        --i;
                }
            }

            return ordering.Select(ordered => ordered.item).ToList();
        }

        #endregion

        [Shape]
        public void HeadScripts(dynamic Display, TextWriter Output) {
            WriteResources(Display, Output, "script", ResourceLocation.Head, null);
            WriteLiteralScripts(Output, _resourceManager.Value.GetRegisteredHeadScripts());
        }

        [Shape]
        public void FootScripts(dynamic Display, TextWriter Output) {
            WriteResources(Display, Output, "script", null, ResourceLocation.Head);
            WriteLiteralScripts(Output, _resourceManager.Value.GetRegisteredFootScripts());
        }

        [Shape]
        public void Metas(TextWriter Output) {
            foreach (var meta in _resourceManager.Value.GetRegisteredMetas() ) {
                Output.WriteLine(meta.GetTag());
            }
        }

        [Shape]
        public void HeadLinks(TextWriter Output) {
            foreach (var link in _resourceManager.Value.GetRegisteredLinks() ) {
                Output.WriteLine(link.GetTag());
            }
        }

        [Shape]
        public void StylesheetLinks(dynamic Display, TextWriter Output) {
            WriteResources(Display, Output, "stylesheet", null, null);
        }

        [Shape]
        public void Style(TextWriter Output, ResourceDefinition Resource, string Url, string Condition, Dictionary<string, string> TagAttributes) {
            UI.Resources.ResourceManager.WriteResource(_virtualPathProvider, Output, Resource, Url, Condition, TagAttributes);
        }

        [Shape]
        public void Resource(TextWriter Output, ResourceDefinition Resource, string Url, string Condition, Dictionary<string, string> TagAttributes) {
            UI.Resources.ResourceManager.WriteResource(_virtualPathProvider, Output, Resource, Url, Condition, TagAttributes);
        }

        private static void WriteLiteralScripts(TextWriter output, IEnumerable<string> scripts) {
            if (scripts == null) {
                return;
            }
            foreach (string script in scripts) {
                output.WriteLine(script);
            }
        }

        private void WriteResources(dynamic Display, TextWriter Output, string resourceType, ResourceLocation? includeLocation, ResourceLocation? excludeLocation) {
            bool debugMode;
            var site = _workContext.Value.CurrentSite;
            switch (site.ResourceDebugMode) {
                case ResourceDebugMode.Enabled:
                    debugMode = true;
                    break;
                case ResourceDebugMode.Disabled:
                    debugMode = false;
                    break;
                default:
                    Debug.Assert(site.ResourceDebugMode == ResourceDebugMode.FromAppSetting, "Unknown ResourceDebugMode value.");
                    debugMode = _httpContextAccessor.Value.Current().IsDebuggingEnabled;
                    break;
            }
            var defaultSettings = new RequireSettings {
                DebugMode = debugMode,
                Culture = CultureInfo.CurrentUICulture.Name,
            };
            var requiredResources = _resourceManager.Value.BuildRequiredResources(resourceType);
            var appPath = _httpContextAccessor.Value.Current().Request.ApplicationPath;
            foreach (var context in requiredResources.Where(r =>
                (includeLocation.HasValue ? r.Settings.Location == includeLocation.Value : true) &&
                (excludeLocation.HasValue ? r.Settings.Location != excludeLocation.Value : true))) {

                var path = context.GetResourceUrl(defaultSettings, appPath);
                var condition = context.Settings.Condition;
                var attributes = context.Settings.HasAttributes ? context.Settings.Attributes : null;
                IHtmlString result;
                if (resourceType == "stylesheet") {
                    result = Display.Style(Url: path, Condition: condition, Resource: context.Resource, TagAttributes: attributes);
                }
                else {
                    result = Display.Resource(Url: path, Condition: condition, Resource: context.Resource, TagAttributes: attributes);
                }
                Output.Write(result);
            }
        }

        [Shape]
        public IHtmlString Image(dynamic Display, dynamic Shape, UrlHelper Url, string Src, object Alt, object Title) {
            // Displays an image. The Src will be resolved against the current context if need be.
            if (Src == null) {
                throw new ArgumentNullException("Src");
            }
            Src = Src == "" ? "" : Url.Content(Src);
            var tag = _tagBuilderFactory.Create((object)Shape, "img");
            tag.MergeAttribute("src", Src);
            var alt = Alt == null ? "" : (string)Display(Alt).ToString();
            tag.MergeAttribute("alt", alt);
            if (Title != null) {
                tag.MergeAttribute("title", (string)Display(Title).ToString());
            }
            if (tag.Attributes.ContainsKey("alt") && !tag.Attributes.ContainsKey("title")) {
                tag.MergeAttribute("title", tag.Attributes["alt"] ?? "");
            }

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.SelfClosing));
        }

        [Shape]
        public IHtmlString UnsafeActionLink(dynamic Display, dynamic Shape, string UrlType) {
            if (string.IsNullOrEmpty(UrlType)) {
                throw new ArgumentNullException("UrlType");
            }
            _resourceManager.Value.Require("script", "UnsafeAction");
            var attributes = (IDictionary<string, string>)Shape.Attributes;
            if (attributes.ContainsKey("itemprop")) {
                attributes["itemprop"] = attributes["itemprop"] + " " + UrlType + " UnsafeUrl";
            }
            else {
                attributes["itemprop"] = UrlType + " UnsafeUrl";
            }

            Shape.Metadata.Type = "ActionLink";
            Shape.Metadata.Alternates.Clear();
            return Display(Shape);
        }

        [Shape]
        public IHtmlString ActionLink(dynamic Display,
            dynamic Shape,
            string Action,
            string Controller,
            string Area
            // parameter omitted to workaround an issue where a NullRef is thrown
            // when an anonymous object is bound to an object shape parameter
            /*, object RouteValues*/) {

            if ((Action ?? Controller ?? Area) != null) {
                // workaround: get it from the shape instead of parameter
                var RouteValues = (object)Shape.RouteValues;

                // Action, Controller, and Area may have been provided directly as
                // a shortcut to providing a RouteValues object. Add them to the
                // RouteValues if provided, or create one if not.
                RouteValueDictionary rvd;
                if (RouteValues == null) {
                    rvd = new RouteValueDictionary();
                }
                else {
                    rvd = RouteValues is RouteValueDictionary ? (RouteValueDictionary)RouteValues : new RouteValueDictionary(RouteValues);
                }
                if (Action != null) {
                    rvd["Action"] = Action;
                    Shape.Action = null;
                }
                if (Controller != null) {
                    rvd["Controller"] = Controller;
                    Shape.Controller = null;
                }
                if (Area != null) {
                    rvd["Area"] = Area;
                    Shape.Area = null;
                }
                Shape.RouteValues = rvd;
            }

            Shape.Metadata.Type = "Link";
            Shape.Metadata.Alternates.Clear();
            return Display(Shape);
        }

        [Shape]
        public IHtmlString Link(HtmlHelper Html,
            UrlHelper Url,
            dynamic Display,
            dynamic Shape,
            string Href,
            string Fragment,
            // parameter omitted to workaround an issue where a NullRef is thrown
            // when an anonymous object is bound to an object shape parameter
            /*object RouteValues,*/
            object Value) {

            // workaround: get it from the shape instead of parameter
            var RouteValues = (object)Shape.RouteValues;
            var href = Href;
            if (href == null && RouteValues != null) {
                // If RouteValues is an actual RouteValueDictionary, be sure and use the correct RouteUrl override, lest it be treated like an anonymous object would be.
                if (RouteValues is RouteValueDictionary) {
                    href = Url.Action(null, (RouteValueDictionary)RouteValues);
                }
                else {
                    href = Url.Action(null, RouteValues);
                }
            }
            if (!string.IsNullOrEmpty(Fragment)) {
                href += "#" + Fragment;
            }

            var tag = _tagBuilderFactory.Create((object)Shape, "a");
            tag.MergeAttribute("href", href);
            tag.InnerHtml = Html.Encode(Value is string ? (string)Value : Display(Value));
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        [Shape]
        public IHtmlString Pager_Links(dynamic Shape, dynamic Display,
            int Page,
            int PageSize,
            double TotalItemCount,
            int Quantity,
            string FirstText,
            string PreviousText,
            string NextText,
            string LastText,
            string GapText
            // parameter omitted to workaround an issue where a NullRef is thrown
            // when an anonymous object is bound to an object shape parameter
            /*object RouteValues*/) {

            var currentPage = Page;
            if (currentPage < 1)
                currentPage = 1;

            var pageSize = PageSize;
            if (pageSize < 1)
                pageSize = _workContext.Value.CurrentSite.PageSize;

            var numberOfPagesToShow = Quantity;
            if (numberOfPagesToShow < 0)
                numberOfPagesToShow = 7;
    
            var totalPageCount = Math.Ceiling(TotalItemCount / pageSize);

            var firstText = !string.IsNullOrWhiteSpace(FirstText) ? FirstText : T("<<").Text;
            var previousText = !string.IsNullOrWhiteSpace(PreviousText) ? PreviousText : T("<").Text;
            var nextText = !string.IsNullOrWhiteSpace(NextText) ? NextText : T(">").Text;
            var lastText = !string.IsNullOrWhiteSpace(LastText) ? LastText : T(">>").Text;
            var gapText = !string.IsNullOrWhiteSpace(GapText) ? GapText : T("...").Text;

            // workaround: get it from the shape instead of parameter
            var RouteValues = (object)Shape.RouteValues;
            var RouteData = RouteValues is RouteValueDictionary ? (RouteValueDictionary) RouteValues : new RouteValueDictionary(RouteValues);
            var queryString = _workContext.Value.HttpContext.Request.QueryString;
            if (queryString != null) {
                foreach (var key in from string key in queryString.Keys where key != null && !RouteData.ContainsKey(key) let value = queryString[key] select key) {
                    RouteData[key] = queryString[key];
                }
            }
    
            if (Shape.RouteData != null) {
                var shapeRouteData = Shape.RouteData is RouteValueDictionary ? (RouteValueDictionary) RouteValues : new RouteValueDictionary(RouteValues);
                foreach (var rd in shapeRouteData) {
                    shapeRouteData[rd.Key] = rd.Value;
                }
            }

            if (RouteData.ContainsKey("id"))
                RouteData.Remove("id");


            var firstPage = Math.Max(1, Page - (numberOfPagesToShow / 2));
            var lastPage = Math.Min(totalPageCount, Page + (numberOfPagesToShow / 2));
    

            Shape.Classes.Add("pager");
            Shape.Metadata.Type = "List";

            // first and previous pages
            if (Page > 1) {
                if (RouteData.ContainsKey("page")) {
                    RouteData.Remove("page"); // to keep from having "page=1" in the query string
                }
                // first
                Shape.Add(Display.Pager_First(Value: firstText, RouteValues: RouteData));
                // previous
                if (currentPage > 2) { // also to keep from having "page=1" in the query string
                    RouteData["page"] = currentPage - 1;
                }
                Shape.Add(Display.Pager_Previous(Value: previousText, RouteValues: RouteData));
            }

            // gap at the beginning of the pager
            if (firstPage > 1) {
                Shape.Add(Display.Pager_Gap(Value: gapText));
            }

            // page numbers
            if (numberOfPagesToShow > 0) {
                for (var p = firstPage; p <= lastPage; p++) {
                    if (p == currentPage) {
                        Shape.Add(Display.Pager_CurrentPage(Value: p, RouteValues: RouteData));
                    }
                    else {
                        if (p == 1)
                            RouteData.Remove("page");
                        else
                            RouteData["page"] = p;
                        Shape.Add(Display.Pager_Link(Value: p, RouteValues: RouteData));
                    }
                }
            }

            // gap at the end of the pager
            if (lastPage < totalPageCount) {
                Shape.Add(Display.Pager_Gap(Value: gapText));
            }
    
            // next and last pages
            if (Page < totalPageCount) {
                // next
                RouteData["page"] = Page + 1;
                Shape.Add(Display.Pager_Next(Value: nextText, RouteValues: RouteData));
                // last
                RouteData["page"] = totalPageCount;
                Shape.Add(Display.Pager_Last(Value: lastText, RouteValues: RouteData));
            }

            return Display(Shape);
        }

        [Shape]
        public IHtmlString Pager_First(dynamic Shape, dynamic Display) {
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        [Shape]
        public IHtmlString Pager_Previous(dynamic Shape, dynamic Display) {
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        [Shape]
        public IHtmlString Pager_CurrentPage(HtmlHelper Html, dynamic Display, object Value) {
            return MvcHtmlString.Create(Html.Encode(Value is string ? (string)Value : Display(Value)));
        }
        [Shape]
        public IHtmlString Pager_Next(dynamic Shape, dynamic Display) {
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        [Shape]
        public IHtmlString Pager_Last(dynamic Shape, dynamic Display) {
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        [Shape]
        public IHtmlString Pager_Link(dynamic Shape, dynamic Display) {
            Shape.Metadata.Type = "ActionLink";
            return Display(Shape);
        }
        [Shape]
        public IHtmlString Pager_Gap(HtmlHelper Html, dynamic Display, object Value) {
            return MvcHtmlString.Create(Html.Encode(Value is string ? (string)Value : Display(Value)));
        }

        [Shape]
        public void List(
            dynamic Display,
            TextWriter Output,
            IEnumerable<dynamic> Items,
            string Tag,
            string Id,
            object Classes,
            IDictionary<string, string> Attributes,
            object ItemClasses,
            object AlternatingItemClasses,
            IDictionary<string, string> ItemAttributes) {

            if (Items == null)
                return;

            var count = Items.Count();
            if (count < 1)
                return;

            var listTagName = string.IsNullOrEmpty(Tag) ? "ul" : Tag;
            const string itemTagName = "li";

            var listTag = GetTagBuilder(listTagName, Id, Classes, null, Attributes);
            Output.Write(listTag.ToString(TagRenderMode.StartTag));

            var index = 0;
            foreach (var item in Items) {
                var itemTag = GetTagBuilder(itemTagName, null, ItemClasses, (index % 2 == 0) ? null : AlternatingItemClasses, ItemAttributes);
                if (index == 0)
                    itemTag.AddCssClass("first");
                if (index == count - 1)
                    itemTag.AddCssClass("last");
                Output.Write(itemTag.ToString(TagRenderMode.StartTag));
                Output.Write(Display(item));
                Output.Write(itemTag.ToString(TagRenderMode.EndTag));
                ++index;
            }

            Output.Write(listTag.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public IHtmlString PlaceChildContent(dynamic Source) {
            return Source.Metadata.ChildContent;
        }

        [Shape]
        public void Partial(HtmlHelper Html, TextWriter Output, string TemplateName, object Model, string Prefix) {
            RenderInternal(Html, Output, TemplateName, Model, Prefix);
        }

        [Shape]
        public void DisplayTemplate(HtmlHelper Html, TextWriter Output, string TemplateName, object Model, string Prefix) {
            RenderInternal(Html, Output, "DisplayTemplates/" + TemplateName, Model, Prefix);
        }

        [Shape]
        public void EditorTemplate(HtmlHelper Html, TextWriter Output, string TemplateName, object Model, string Prefix) {
            RenderInternal(Html, Output, "EditorTemplates/" + TemplateName, Model, Prefix);
        }

        static void RenderInternal(HtmlHelper Html, TextWriter Output, string TemplateName, object Model, string Prefix) {
            var adjustedViewData = new ViewDataDictionary(Html.ViewDataContainer.ViewData) {
                Model = DetermineModel(Html, Model),
                TemplateInfo = new TemplateInfo {
                    HtmlFieldPrefix = DeterminePrefix(Html, Prefix)
                }
            };
            var adjustedViewContext = new ViewContext(Html.ViewContext, Html.ViewContext.View, adjustedViewData, Html.ViewContext.TempData, Output);
            var adjustedHtml = new HtmlHelper(adjustedViewContext, new ViewDataContainer(adjustedViewData));
            adjustedHtml.RenderPartial(TemplateName);
        }

        static object DetermineModel(HtmlHelper Html, object Model) {
            bool isNull = ((dynamic)Model) == null;
            return isNull ? Html.ViewData.Model : Model;
        }

        static string DeterminePrefix(HtmlHelper Html, string Prefix) {
            var actualPrefix = string.IsNullOrEmpty(Prefix)
                                   ? Html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix
                                   : Html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(Prefix);
            return actualPrefix;
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataContainer(ViewDataDictionary viewData) { ViewData = viewData; }
            public ViewDataDictionary ViewData { get; set; }
        }

    }
}
