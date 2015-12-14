using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ResourceBindingStrategy;
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
        private readonly Work<IShapeFactory> _shapeFactory;

        public CoreShapes(
            Work<WorkContext> workContext, 
            Work<IResourceManager> resourceManager,
            Work<IHttpContextAccessor> httpContextAccessor,
            Work<IShapeFactory> shapeFactory
            ) {
            _workContext = workContext;
            _resourceManager = resourceManager;
            _httpContextAccessor = httpContextAccessor;
            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public dynamic New { get { return _shapeFactory.Value;  } }

        public void Discover(ShapeTableBuilder builder) {
            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe("Layout")
                .Configure(descriptor => descriptor.Wrappers.Add("Document"))
                .OnCreating(creating => creating.Create = () => new ZoneHolding(() => creating.New.Zone()))
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
                .OnCreating(creating => creating.Create = () => new Zone())
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
                    menu.Metadata.Alternates.Add("Menu__" + EncodeAlternateElement(menuName));
                });

            builder.Describe("MenuItem")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    int level = menuItem.Level;

                    menuItem.Metadata.Alternates.Add("MenuItem__level__" + level);
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menu.MenuName));
                    menuItem.Metadata.Alternates.Add("MenuItem__" + EncodeAlternateElement(menu.MenuName) + "__level__" + level);
                });

            builder.Describe("MenuItemLink")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    string menuName = menuItem.Menu.MenuName;
                    string contentType = null;
                    int level = menuItem.Level;
                    if (menuItem.Content != null) {
                        contentType = ((IContent) menuItem.Content).ContentItem.ContentType;
                    }

                    menuItem.Metadata.Alternates.Add("MenuItemLink__level__" + level);

                    // MenuItemLink__[ContentType] e.g. MenuItemLink-HtmlMenuItem
                    // MenuItemLink__[ContentType]__level__[level] e.g. MenuItemLink-HtmlMenuItem-level-2
                    if (contentType != null) {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    }

                    // MenuItemLink__[MenuName] e.g. MenuItemLink-Main-Menu
                    // MenuItemLink__[MenuName]__level__[level] e.g. MenuItemLink-Main-Menu-level-2
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName));
                    menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__level__" + level);

                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem
                    // MenuItemLink__[MenuName]__[ContentType] e.g. MenuItemLink-Main-Menu-HtmlMenuItem-level-2
                    if (contentType != null) {
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType));
                        menuItem.Metadata.Alternates.Add("MenuItemLink__" + EncodeAlternateElement(menuName) + "__" + EncodeAlternateElement(contentType) + "__level__" + level);
                    }
                });

            builder.Describe("LocalMenu")
                .OnDisplaying(displaying => {
                    var menu = displaying.Shape;
                    string menuName = menu.MenuName;
                    menu.Classes.Add("localmenu-" + menuName.HtmlClassify());
                    menu.Classes.Add("localmenu");
                    menu.Metadata.Alternates.Add("LocalMenu__" + EncodeAlternateElement(menuName));
                });
            
            builder.Describe("LocalMenuItem")
                .OnDisplaying(displaying => {
                    var menuItem = displaying.Shape;
                    var menu = menuItem.Menu;
                    menuItem.Metadata.Alternates.Add("LocalMenuItem__" + EncodeAlternateElement(menu.MenuName));
                });

            #region Pager alternates
            builder.Describe("Pager")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Gap")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        pager.Metadata.Alternates.Add("Pager_Gap__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_First")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_First__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Previous")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Previous__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Next")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Next__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Last")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Last__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_CurrentPage")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape.Pager;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_CurrentPage__" + EncodeAlternateElement(pagerId));
                });

            builder.Describe("Pager_Links")
                .OnDisplaying(displaying => {
                    var pager = displaying.Shape;
                    string pagerId = pager.PagerId;
                    if (!String.IsNullOrWhiteSpace(pagerId))
                        displaying.Shape.Metadata.Alternates.Add("Pager_Links__" + EncodeAlternateElement(pagerId));
                });

            #endregion

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
                    var fileName = url != null ? StaticFileBindingStrategy.GetAlternateShapeNameFromFileName(url) : default(string);
                    if (!string.IsNullOrEmpty(fileName)) {
                        resource.Metadata.Alternates.Add("Style__" + fileName);
                    }
                });

            builder.Describe("Script")
                .OnDisplaying(displaying => {
                    var resource = displaying.Shape;
                    string url = resource.Url;
                    var fileName = url != null ? StaticFileBindingStrategy.GetAlternateShapeNameFromFileName(url) : default(string);
                    if (!string.IsNullOrEmpty(fileName)) {
                        resource.Metadata.Alternates.Add("Script__" + fileName);
                    }
                });

            builder.Describe("Resource")
                .OnDisplaying(displaying => {
                    var resource = displaying.Shape;
                    string url = resource.Url;
                    var fileName = url != null ? StaticFileBindingStrategy.GetAlternateShapeNameFromFileName(url) : default(string);
                    if (!string.IsNullOrEmpty(fileName)) {
                        resource.Metadata.Alternates.Add("Resource__" + fileName);
                    }
                });
        }


        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (!string.IsNullOrWhiteSpace(id))
                tagBuilder.GenerateId(id);
            return tagBuilder;
        }

        [Shape]
        public void Zone(dynamic Display, dynamic Shape, TextWriter Output) {
            string id = Shape.Id;
            IEnumerable<string> classes = Shape.Classes;
            IDictionary<string, string> attributes = Shape.Attributes;
            var zoneWrapper = GetTagBuilder("div", id, classes, attributes);
            Output.Write(zoneWrapper.ToString(TagRenderMode.StartTag));
            foreach (var item in Order(Shape))
                Output.Write(Display(item));
            Output.Write(zoneWrapper.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void ContentZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in Order(Shape))
                Output.Write(Display(item));
        }

        [Shape]
        public void DocumentZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in Order(Shape))
                Output.Write(Display(item));
        }

        public static IEnumerable<dynamic> Order(dynamic shape) {
            IEnumerable<dynamic> unordered = shape;
            if (unordered == null || unordered.Count() < 2)
                return shape;

            var i = 1;
            var progress = 1;
            var flatPositionComparer = new FlatPositionComparer();
            var ordering = unordered.Select(item => {
                string position = null;
                var itemPosition = item as IPositioned;
                if (itemPosition != null) {
                    position = itemPosition.Position;
                }

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
        public void Style(dynamic Display, HtmlHelper Html, TextWriter Output, ResourceDefinition Resource, string Url, string Condition, Dictionary<string, string> TagAttributes) {
            // do not write to Output directly as Styles are rendered in Zones
            ResourceManager.WriteResource(Html.ViewContext.Writer, Resource, Url, Condition, TagAttributes);
        }

        [Shape]
        public void Script(HtmlHelper Html, TextWriter Output, ResourceDefinition Resource, string Url, string Condition, Dictionary<string, string> TagAttributes) {
            // do not write to Output directly as Styles are rendered in Zones
            ResourceManager.WriteResource(Html.ViewContext.Writer, Resource, Url, Condition, TagAttributes);
        }

        [Shape]
        public void Resource(TextWriter Output, ResourceDefinition Resource, string Url, string Condition, Dictionary<string, string> TagAttributes) {
            ResourceManager.WriteResource(Output, Resource, Url, Condition, TagAttributes);
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
                CdnMode = site.UseCdn,
                Culture = _workContext.Value.CurrentCulture,
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
                else if (resourceType == "script") { 
                    result = Display.Script(Url: path, Condition: condition, Resource: context.Resource, TagAttributes: attributes);
                }
                else {
                    result = Display.Resource(Url: path, Condition: condition, Resource: context.Resource, TagAttributes: attributes);
                }
                Output.Write(result);
            }
        }

        [Shape]
        public IHtmlString Pager_Links(dynamic Shape, dynamic Display,
            HtmlHelper Html,
            int Page,
            int PageSize,
            double TotalItemCount,
            int? Quantity,
            object FirstText,
            object PreviousText,
            object NextText,
            object LastText,
            object GapText,
            string PagerId
            // parameter omitted to workaround an issue where a NullRef is thrown
            // when an anonymous object is bound to an object shape parameter
            /*object RouteValues*/) {

            var currentPage = Page;
            if (currentPage < 1)
                currentPage = 1;

            var pageSize = PageSize;
            
            var numberOfPagesToShow = Quantity ?? 0;
            if (Quantity == null || Quantity < 0)
                numberOfPagesToShow = 7;
    
            var totalPageCount = pageSize > 0 ? (int)Math.Ceiling(TotalItemCount / pageSize) : 1;

            var firstText = FirstText ?? T("&lt;&lt;");
            var previousText = PreviousText ?? T("&lt;");
            var nextText = NextText ?? T("&gt;");
            var lastText = LastText ?? T("&gt;&gt;");
            var gapText = GapText ?? T("...");

            var routeData = new RouteValueDictionary(Html.ViewContext.RouteData.Values);
            var queryString = _workContext.Value.HttpContext.Request.QueryString;
            if (queryString != null) {
                foreach (var key in from string key in queryString.Keys where key != null && !routeData.ContainsKey(key) let value = queryString[key] select key) {
                    routeData[key] = queryString[key];
                }
            }

            // specific cross-requests route data can be passed to the shape directly (e.g., Orchard.Users)
            var shapeRoute = (object)Shape.RouteData;

            if (shapeRoute != null) {
                var shapeRouteData = shapeRoute as RouteValueDictionary;
                if (shapeRouteData == null) {
                    var route = shapeRoute as RouteData;
                    if (route != null) {
                        shapeRouteData = (route).Values;    
                    }
                }

                if (shapeRouteData != null) {
                    foreach (var rd in shapeRouteData) {
                        routeData[rd.Key] = rd.Value;
                    }
                }
            }

            // HACK: MVC 3 is adding a specific value in System.Web.Mvc.Html.ChildActionExtensions.ActionHelper
            // when a content item is set as home page, it is rendered by using Html.RenderAction, and the routeData is altered
            // This code removes this extra route value
            var removedKeys = routeData.Keys.Where(key => routeData[key] is DictionaryValueProvider<object>).ToList();
            foreach (var key in removedKeys) {
                routeData.Remove(key);
            }

            int firstPage = Math.Max(1, Page - (numberOfPagesToShow / 2));
            int lastPage = Math.Min(totalPageCount, Page + (int)(numberOfPagesToShow / 2));

            var pageKey = String.IsNullOrEmpty(PagerId) ? "page" : PagerId;

            Shape.Classes.Add("pager");
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "List";

            // first and previous pages
            if (Page > 1) {
                if (routeData.ContainsKey(pageKey)) {
                    routeData.Remove(pageKey); // to keep from having "page=1" in the query string
                }
                // first
                Shape.Add(New.Pager_First(Value: firstText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));

                // previous
                if (currentPage > 2) { // also to keep from having "page=1" in the query string
                    routeData[pageKey] = currentPage - 1;
                }
                Shape.Add(New.Pager_Previous(Value: previousText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
            }

            // gap at the beginning of the pager
            if (firstPage > 1 && numberOfPagesToShow > 0) {
                Shape.Add(New.Pager_Gap(Value: gapText, Pager: Shape));
            }

            // page numbers
            if (numberOfPagesToShow > 0 && lastPage > 1) {
                for (var p = firstPage; p <= lastPage; p++) {
                    if (p == currentPage) {
                        Shape.Add(New.Pager_CurrentPage(Value: p, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                    }
                    else {
                        if (p == 1)
                            routeData.Remove(pageKey);
                        else
                            routeData[pageKey] = p;
                        Shape.Add(New.Pager_Link(Value: p, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                    }
                }
            }

            // gap at the end of the pager
            if (lastPage < totalPageCount && numberOfPagesToShow > 0) {
                Shape.Add(New.Pager_Gap(Value: gapText, Pager: Shape));
            }
    
            // next and last pages
            if (Page < totalPageCount) {
                // next
                routeData[pageKey] = Page + 1;
                Shape.Add(New.Pager_Next(Value: nextText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
                // last
                routeData[pageKey] = totalPageCount;
                Shape.Add(New.Pager_Last(Value: lastText, RouteValues: new RouteValueDictionary(routeData), Pager: Shape));
            }

            return Display(Shape);
        }

        [Shape]
        public IHtmlString Pager(dynamic Shape, dynamic Display) {
            Shape.Metadata.Alternates.Clear(); 
            Shape.Metadata.Type = "Pager_Links";
            return Display(Shape);
        }

        [Shape]
        public IHtmlString Pager_First(dynamic Shape, dynamic Display) {
            Shape.Metadata.Alternates.Clear(); 
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        
        [Shape]
        public IHtmlString Pager_Previous(dynamic Shape, dynamic Display) {
            Shape.Metadata.Alternates.Clear(); 
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        
        [Shape]
        public IHtmlString Pager_CurrentPage(HtmlHelper Html, dynamic Display, object Value) {
            var tagBuilder = new TagBuilder("span");
            tagBuilder.InnerHtml = EncodeOrDisplay(Value, Display, Html).ToString();
            
            return MvcHtmlString.Create(tagBuilder.ToString());
        }
        
        [Shape]
        public IHtmlString Pager_Next(dynamic Shape, dynamic Display) {
            Shape.Metadata.Alternates.Clear(); 
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        
        [Shape]
        public IHtmlString Pager_Last(dynamic Shape, dynamic Display) {
            Shape.Metadata.Alternates.Clear(); 
            Shape.Metadata.Type = "Pager_Link";
            return Display(Shape);
        }
        
        [Shape]
        public IHtmlString Pager_Link(HtmlHelper Html, dynamic Shape, dynamic Display, object Value) {
            Shape.Metadata.Alternates.Clear();
            Shape.Metadata.Type = "ActionLink";
            return Display(Shape);
        }

        [Shape]
        public IHtmlString ActionLink(HtmlHelper Html, UrlHelper Url, dynamic Shape, dynamic Display, object Value) {
            var RouteValues = (object)Shape.RouteValues;
            RouteValueDictionary rvd;
            if (RouteValues == null) {
                rvd = new RouteValueDictionary();
            }
            else {
                rvd = RouteValues is RouteValueDictionary ? (RouteValueDictionary)RouteValues : new RouteValueDictionary(RouteValues);
            }

            var action = Url.Action((string)rvd["action"], (string)rvd["controller"], rvd);

            IEnumerable<string> classes = Shape.Classes;
            IDictionary<string, string> attributes = Shape.Attributes;
            attributes["href"] = action;
            string id = Shape.Id;
            var tag = GetTagBuilder("a", id, classes, attributes);
            tag.InnerHtml = EncodeOrDisplay(Value, Display, Html).ToString();

            return Html.Raw(tag.ToString());
        }

        [Shape]
        public IHtmlString Pager_Gap(HtmlHelper Html, dynamic Display, object Value) {
            var tagBuilder = new TagBuilder("span");
            tagBuilder.InnerHtml = EncodeOrDisplay(Value, Display, Html).ToString();

            return MvcHtmlString.Create(tagBuilder.ToString());
        }

        [Shape]
        public void List(
            dynamic Display,
            TextWriter Output,
            IEnumerable<dynamic> Items,
            string Tag,
            string Id,
            IEnumerable<string> Classes,
            IDictionary<string, string> Attributes,
            string ItemTag,
            IEnumerable<string> ItemClasses,
            IDictionary<string, string> ItemAttributes) {

            if (Items == null)
                return;
            
            // prevent multiple enumerations
            var items = Items.ToList();

            // var itemDisplayOutputs = Items.Select(item => Display(item)).Where(output => !string.IsNullOrWhiteSpace(output.ToHtmlString())).ToList();
            var count = items.Count();
            if (count < 1)
                return;

            string listTagName = null;
            
            if (Tag != "-") {
                listTagName = string.IsNullOrEmpty(Tag) ? "ul" : Tag;
            }

            var listTag = String.IsNullOrEmpty(listTagName) ? null : GetTagBuilder(listTagName, Id, Classes, Attributes);

            string itemTagName = null;
            if (ItemTag != "-") {
             itemTagName = string.IsNullOrEmpty(ItemTag) ? "li" : ItemTag;
            }

            if (listTag != null) {
                Output.Write(listTag.ToString(TagRenderMode.StartTag));
            }

            var itemTags = new List<TagBuilder>();
            var itemOutputs = new List<string>();

            // give the item shape the possibility to alter its container tag
            var index = 0;
            foreach (var item in items) {
                
                var itemTag = String.IsNullOrEmpty(itemTagName) ? null : GetTagBuilder(itemTagName, null, ItemClasses, ItemAttributes);

                if (item is IShape) {
                    item.Tag = itemTag;
                }

                var itemOutput = Display(item).ToHtmlString();

                if (!String.IsNullOrWhiteSpace(itemOutput)) {
                    itemTags.Add(itemTag);
                    itemOutputs.Add(itemOutput);
                }
                else {
                    count--;
                }

                ++index;
            }

            index = 0;
            foreach(var itemOutput in itemOutputs) {
                var itemTag = itemTags[index];

                if (itemTag != null) {
                    if (index == 0)
                        itemTag.AddCssClass("first");
                    if (index == count - 1)
                        itemTag.AddCssClass("last");
                    Output.Write(itemTag.ToString(TagRenderMode.StartTag));
                }

                Output.Write(itemOutput);

                if (itemTag != null) {
                    Output.WriteLine(itemTag.ToString(TagRenderMode.EndTag));
                }

                ++index;
            }

            if (listTag != null) {
                Output.WriteLine(listTag.ToString(TagRenderMode.EndTag));
            }
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

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames 
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private string EncodeAlternateElement(string alternateElement) {
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }

        /// <summary>
        /// Encode a value if it's a string, or render it if it's a Shape
        /// </summary>
        private IHtmlString EncodeOrDisplay(dynamic Value, dynamic Display, HtmlHelper Html) {
            if (Value is IHtmlString) {
                return Value;
            }

            if (Value is IShape) {
                return Display(Value).ToString();
            }
            
            return Html.Raw(Html.Encode(Value.ToString()));
        }
    }
}
