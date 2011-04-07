using HtmlAgilityPack;

namespace Orchard.Specs.Bindings {
    public static class HtmlNodeExtensions {
        public static string GetOptionValue(this HtmlNode node) {
            return node.Attributes.Contains("value")
                       ? node.GetAttributeValue("value", "")
                       : node.NextSibling != null && node.NextSibling.NodeType == HtmlNodeType.Text ? node.NextSibling.InnerText : "";
        }
    }
}