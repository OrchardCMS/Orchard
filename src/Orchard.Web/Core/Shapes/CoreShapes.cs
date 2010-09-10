using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions.Models;
using Orchard.UI;
using Orchard.UI.Zones;

// ReSharper disable InconsistentNaming

namespace Orchard.Core.Shapes {
    public class CoreShapes : IShapeDescriptorBindingStrategy {
        public Feature Feature { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe.Named("Layout").From(Feature.Descriptor)
                .Configure(descriptor => descriptor.Wrappers.Add("Document"))
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(creating.ShapeFactory)))
                .OnCreated(created => {
                    created.Shape.Zones.Content.Add(created.New.PlaceChildContent(Source: created.Shape), "5");
                    created.Shape.Zones.Body.Add(created.New.PlaceChildContent(Source: created.Shape), "5");
                });

            // 'Zone' shapes are built on the Zone base class
            builder.Describe.Named("Zone").From(Feature.Descriptor)
                .OnCreating(creating => creating.BaseType = typeof(Zone));

            // 'List' shapes start with several empty collections
            builder.Describe.Named("List").From(Feature.Descriptor)
                .OnCreated(created => {
                    created.Shape.Tag = "ul";
                    created.Shape.Classes = new List<string>();
                    created.Shape.Attributes = new Dictionary<string, string>();
                    created.Shape.ItemClasses = new List<string>();
                    created.Shape.ItemAttributes = new Dictionary<string, string>();
                });


        }


        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (id != null)
                tagBuilder.GenerateId(id);
            return tagBuilder;
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
