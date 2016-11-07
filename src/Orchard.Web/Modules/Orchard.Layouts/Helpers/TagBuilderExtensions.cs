using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.DisplayManagement.Shapes;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Helpers {
    public static class TagBuilderExtensions {

        /// <summary>
        /// Creates an <see cref="OrchardTagBuilder"/> and adds the ID, Class and Style attributes from the shape.Element property.
        /// </summary>
        public static OrchardTagBuilder CreateElementTagBuilder(dynamic shape, string tag = "div") {
            return AddCommonElementAttributes(new OrchardTagBuilder(tag), shape);
        }

        public static OrchardTagBuilder AddCommonElementAttributes(this OrchardTagBuilder tagBuilder, dynamic shape) {
            var attributes = GetCommonElementAttributes(shape);
            tagBuilder.MergeAttributes(shape.Attributes);
            tagBuilder.MergeAttributes(attributes);
            return tagBuilder;
        }

        public static IDictionary<string, object> GetCommonElementAttributes(dynamic shape) {
            var element = (Element)shape.Element;
            var htmlId = element.HtmlId;
            var htmlClass = element.HtmlClass;
            var htmlStyle = element.HtmlStyle;
            var attributes = new Dictionary<string, object>();

            if (!String.IsNullOrWhiteSpace(htmlId)) {
                var tokenize = (Func<string>)shape.TokenizeHtmlId;
                attributes["id"] = tokenize();
            }

            if (!String.IsNullOrWhiteSpace(htmlStyle)) {
                var tokenize = (Func<string>)shape.TokenizeHtmlStyle;
                attributes["style"] = Regex.Replace(tokenize(), @"(?:\r\n|[\r\n])", "");
            }

            IList<string> classes = shape.Classes;

            if (!String.IsNullOrWhiteSpace(htmlClass)) {
                var tokenize = (Func<string>)shape.TokenizeHtmlClass;
                var cssClass = tokenize();
                classes.Add(cssClass);
            }

            if (classes.Any())
                attributes["class"] = String.Join(" ", classes);

            return attributes;
        }

        public static void AddClientValidationAttributes(this OrchardTagBuilder tagBuilder, IDictionary<string, string> clientAttributes) {
            foreach (var attribute in clientAttributes) {
                tagBuilder.Attributes[attribute.Key] = attribute.Value;
            }
        }
    }
}
