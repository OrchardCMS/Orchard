using System.Collections;
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
                .OnCreating(creating => creating.Behaviors.Add(new ZoneHoldingBehavior(creating.ShapeFactory)))
                .Configure(descriptor => descriptor.Wrappers.Add("Document"));

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

        static object DetermineModel(HtmlHelper Html, object Model) {
            bool isNull = ((dynamic)Model) == null;
            return isNull ? Html.ViewData.Model : Model;
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
        public IHtmlString Partial(HtmlHelper Html, string TemplateName, object Model) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }

        [Shape]
        public IHtmlString DisplayTemplate(HtmlHelper Html, string TemplateName, object Model, string Prefix) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }

        [Shape]
        public IHtmlString EditorTemplate(HtmlHelper Html, string TemplateName, object Model, string Prefix) {
            return Html.Partial(TemplateName, DetermineModel(Html, Model));
        }

    }
}