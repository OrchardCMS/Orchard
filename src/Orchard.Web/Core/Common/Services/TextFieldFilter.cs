using System;
using System.Web;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Common.Services {
    public class TextFieldFilter : IHtmlFilter {
        public string ProcessContent(string text, string flavor) {
            // Flavor is null for a normal input/text field
            return flavor == null || string.Equals(flavor, "textarea", StringComparison.OrdinalIgnoreCase) ? ReplaceNewLines(text) : text;
        }

        private static string ReplaceNewLines(string text) {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return HttpUtility.HtmlEncode(text).ReplaceNewLinesWith("<br />");
        }
    }
} 