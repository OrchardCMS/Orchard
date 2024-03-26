using System;
using System.Linq;
using System.Web;

namespace Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy {
    public class ContentPartPlacementParseMatchProvider : IPlacementParseMatchProvider {
        public string Key { get { return "ContentPart"; } }

        public bool Match(ShapePlacementContext context, string expression) {
            return context.Content != null && context.Content.ContentItem.Parts.Any(part => part.PartDefinition.Name == expression);
        }
    }

    public class ContentTypePlacementParseMatchProvider : IPlacementParseMatchProvider {
        public string Key { get { return "ContentType"; } }

        public bool Match(ShapePlacementContext context, string expression) {
            if (expression.EndsWith("*")) {
                var prefix = expression.Substring(0, expression.Length - 1);
                return (context.ContentType ?? "").StartsWith(prefix) || (context.Stereotype ?? "").StartsWith(prefix);
            }

            return context.ContentType == expression || context.Stereotype == expression;
        }
    }

    public class DisplayTypePlacementParseMatchProvider : IPlacementParseMatchProvider {
        public string Key { get { return "DisplayType"; } }

        public bool Match(ShapePlacementContext context, string expression) {
            if (expression.EndsWith("*")) {
                var prefix = expression.Substring(0, expression.Length - 1);
                return (context.DisplayType ?? "").StartsWith(prefix);
            }

            return context.DisplayType == expression;
        }
    }

    public class PathPlacementParseMatchProvider : IPlacementParseMatchProvider {
        public string Key { get { return "Path"; } }

        public bool Match(ShapePlacementContext context, string expression) {
            var normalizedPath = VirtualPathUtility.IsAbsolute(expression)
                                            ? VirtualPathUtility.ToAppRelative(expression)
                                            : VirtualPathUtility.Combine("~/", expression);

            if (normalizedPath.EndsWith("*")) {
                var prefix = normalizedPath.Substring(0, normalizedPath.Length - 1);
                return VirtualPathUtility.ToAppRelative(string.IsNullOrEmpty(context.Path) ? "/" : context.Path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
            }

            normalizedPath = VirtualPathUtility.AppendTrailingSlash(normalizedPath);
            return context.Path.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
