using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace Orchard.Projections.Providers.Layouts {
    public class RawShapes : IDependency {
        public RawShapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Raw(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, string Tag, IEnumerable<dynamic> Items, IEnumerable<string> Classes, IDictionary<string, string> Attributes, string ItemTag, IEnumerable<string> ItemClasses, IDictionary<string, string> ItemAttributes, string Prepend, string Append, string Separator) {
            if (Items == null)
                return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1)
                return;
            
            var containerTag = String.IsNullOrEmpty(Tag) ? null : GetTagBuilder(Tag, Id, Classes, Attributes);
            var itemTag = String.IsNullOrEmpty(ItemTag) ? null : GetTagBuilder(ItemTag, string.Empty, ItemClasses, ItemAttributes);

            if (containerTag != null) {
                Output.Write(containerTag.ToString(TagRenderMode.StartTag));
            }

            if (!String.IsNullOrEmpty(Prepend)) {
                Output.Write(Prepend);
            }

            var first = true;
            
            foreach (var item in items) {
                if (!first && !String.IsNullOrEmpty(Separator)) {
                    Output.Write(Separator);
                }

                if (itemTag != null) {
                    Output.Write(itemTag.ToString(TagRenderMode.StartTag));
                }

                Output.Write(Display(item));

                if (itemTag != null) {
                    Output.Write(itemTag.ToString(TagRenderMode.EndTag));
                }

                first = false;
            }

            if (!String.IsNullOrEmpty(Append)) {
                Output.Write(Append);
            }

            if (containerTag != null) {
                Output.Write(containerTag.ToString(TagRenderMode.EndTag));
            }
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

    }
}
