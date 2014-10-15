using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Orchard.DisplayManagement.Shapes;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Settings;

namespace Orchard.Layouts.Helpers {
    public static class TagBuilderExtensions {
        
        public static OrchardTagBuilder AddCommonElementAttributes(this OrchardTagBuilder tagBuilder, dynamic shape) {
            var attributes = GetCommonElementAttributes(shape);
            tagBuilder.MergeAttributes(attributes);
            return tagBuilder;
        }

        public static IDictionary<string, object> GetCommonElementAttributes(dynamic shape) {
            var element = (IElement)shape.Element;
            var settings = element.State ?? new StateDictionary();
            var commonSettings = settings.GetModel<CommonElementSettings>();
            var id = commonSettings.Id;
            var cssClass = commonSettings.CssClass;
            var inlineStyle = commonSettings.InlineStyle;
            var attributes = new Dictionary<string, object>();

            if (!String.IsNullOrWhiteSpace(id)) {
                var tokenize = (Func<string>)shape.TokenizeId;
                attributes["id"] = tokenize();
            }

            if (!String.IsNullOrWhiteSpace(inlineStyle)) {
                var tokenize = (Func<string>)shape.TokenizeInlineStyle;
                attributes["style"] = Regex.Replace(tokenize(), @"(?:\r\n|[\r\n])", "");
            }

            if (!String.IsNullOrWhiteSpace(cssClass)) {
                var tokenize = (Func<string>)shape.TokenizeCssClass;
                attributes["class"] = tokenize();
            }

            return attributes;
        }

        public static void AddClientValidationAttributes(this OrchardTagBuilder tagBuilder, IDictionary<string, string> clientAttributes) {
            foreach (var attribute in clientAttributes) {
                tagBuilder.Attributes[attribute.Key] = attribute.Value;
            }
        }
    }
}