using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Mvc.ViewEngines;
using Orchard.Settings;
using Orchard.UI;
using Orchard.UI.Resources;
using Orchard.UI.Zones;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Shapes {
    public class CoreShapes : IShapeTableProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public CoreShapes(IWorkContextAccessor workContextAccessor) {
            // needed to get CurrentSite.
            // note that injecting ISiteService here causes a stack overflow in AutoFac!
            _workContextAccessor = workContextAccessor;
        }

        public void Discover(ShapeTableBuilder builder) {
            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe("Layout")
                .Configure(descriptor => descriptor.Wrappers.Add("Document"))
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(name => CreateZone(creating, name))))
                .OnCreated(created => {
                    var page = created.Shape;
                    page.Head = created.New.DocumentZone();
                    page.Body = created.New.DocumentZone();
                    page.Tail = created.New.DocumentZone();
                    page.Content = created.New.Zone();

                    page.Body.Add(created.New.PlaceChildContent(Source: page), "5");
                    page.Content.Add(created.New.PlaceChildContent(Source: page), "5");
                });

            // 'Zone' shapes are built on the Zone base class
            builder.Describe("Zone")
                .OnCreating(creating => creating.BaseType = typeof(Zone));

            // 'List' shapes start with several empty collections
            builder.Describe("List")
                .OnCreated(created => {
                    created.Shape.Tag = "ul";
                    created.Shape.ItemClasses = new List<string>();
                    created.Shape.ItemAttributes = new Dictionary<string, string>();
                });
        }

        private object CreateZone(ShapeCreatingContext context, string zoneName) {
            var name = zoneName.ToLower();

            var zone = context.New.Zone();
            zone.Id = "zone-" + name;
            zone.Classes.Add(zone.Id);
            zone.Classes.Add("zone");
            return zone;
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
            foreach (var item in Shape)
                Output.Write(Display(item));
            Output.Write(zoneWrapper.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public void DocumentZone(dynamic Display, dynamic Shape, TextWriter Output) {
            foreach (var item in Shape)
                Output.Write(Display(item));
        }

        [Shape]
        public void HeadScripts(HtmlHelper Html, IResourceManager ResourceManager) {
            WriteResources(Html, _workContextAccessor.GetContext(Html.ViewContext).CurrentSite,
                ResourceManager, "script", ResourceLocation.Head, null);
            WriteLiteralScripts(Html, ResourceManager.GetRegisteredHeadScripts());
        }

        [Shape]
        public void FootScripts(HtmlHelper Html, IResourceManager ResourceManager) {
            WriteResources(Html, _workContextAccessor.GetContext(Html.ViewContext).CurrentSite,
                ResourceManager, "script", null, ResourceLocation.Head);
            WriteLiteralScripts(Html, ResourceManager.GetRegisteredFootScripts());
        }

        [Shape]
        public void Metas(HtmlHelper Html, IResourceManager ResourceManager) {
            foreach (var meta in ResourceManager.GetRegisteredMetas()) {
                Html.ViewContext.Writer.WriteLine(meta.GetTag());
            }
        }

        [Shape]
        public void HeadLinks(HtmlHelper Html, IResourceManager ResourceManager) {
            foreach (var link in ResourceManager.GetRegisteredLinks()) {
                Html.ViewContext.Writer.WriteLine(link.GetTag());
            }
        }

        [Shape]
        public void StylesheetLinks(HtmlHelper Html, IResourceManager ResourceManager) {
            WriteResources(Html, _workContextAccessor.GetContext(Html.ViewContext).CurrentSite,
                ResourceManager, "stylesheet", null, null);
        }

        private static void WriteLiteralScripts(HtmlHelper html, IEnumerable<string> scripts) {
            if (scripts == null) {
                return;
            }
            var writer = html.ViewContext.Writer;
            foreach (string script in scripts) {
                writer.WriteLine(script);
            }
        }

        private static void WriteResources(HtmlHelper html, ISite site, IResourceManager rm, string resourceType, ResourceLocation? includeLocation, ResourceLocation? excludeLocation) {
            bool debugMode;
            switch(site.ResourceDebugMode) {
                case ResourceDebugMode.Enabled:
                    debugMode = true;
                    break;
                case ResourceDebugMode.Disabled:
                    debugMode = false;
                    break;
                default:
                    Debug.Assert(site.ResourceDebugMode == ResourceDebugMode.FromAppSetting, "Unknown ResourceDebugMode value.");
                    debugMode = html.ViewContext.HttpContext.IsDebuggingEnabled;
                    break;
            }
            var defaultSettings = new RequireSettings {
                DebugMode = debugMode,
                Culture = CultureInfo.CurrentUICulture.Name,
            };
            var requiredResources = rm.BuildRequiredResources(resourceType);
            var appPath = html.ViewContext.HttpContext.Request.ApplicationPath;
            foreach (var context in requiredResources.Where(r =>
                (includeLocation.HasValue ? r.Settings.Location == includeLocation.Value : true) &&
                (excludeLocation.HasValue ? r.Settings.Location != excludeLocation.Value : true))) {
                html.ViewContext.Writer.WriteLine(context.GetTagBuilder(defaultSettings, appPath).ToString(context.Resource.TagRenderMode));
            }
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
            IEnumerable<string> ItemClasses,
            IDictionary<string, string> ItemAttributes) {

            var listTagName = string.IsNullOrEmpty(Tag) ? "ul" : Tag;
            const string itemTagName = "li";

            var listTag = GetTagBuilder(listTagName, Id, Classes, Attributes);
            Output.Write(listTag.ToString(TagRenderMode.StartTag));

            if (Items != null) {
                var count = Items.Count();
                var index = 0;
                foreach (var item in Items) {
                    var itemTag = GetTagBuilder(itemTagName, null, ItemClasses, ItemAttributes);
                    if (index == 0)
                        itemTag.AddCssClass("first");
                    if (index == count - 1)
                        itemTag.AddCssClass("last");
                    Output.Write(itemTag.ToString(TagRenderMode.StartTag));
                    Output.Write(Display(item));
                    Output.Write(itemTag.ToString(TagRenderMode.EndTag));
                    ++index;
                }
            }
            Output.Write(listTag.ToString(TagRenderMode.EndTag));
        }

        [Shape]
        public IHtmlString PlaceChildContent(dynamic Source) {
            return Source.Metadata.ChildContent;
        }

        [Shape]
        public void Partial(HtmlHelper Html, TextWriter Output, string TemplateName, object Model) {
            RenderInternal(Html, Output, TemplateName, Model, null);
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
