using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class ContentItemTemplateResult<TContent> : DriverResult where TContent : class, IContent {
        public ContentItemTemplateResult(string templateName) {
            TemplateName = templateName;
        }

        public string TemplateName { get; set; }

        public override void Apply(BuildDisplayContext context) {
        }

        public override void Apply(BuildEditorContext context) {
        }

        class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }

        public ContentItemTemplateResult<TContent> LongestMatch(string displayType, params string[] knownDisplayTypes) {

            if (string.IsNullOrEmpty(displayType))
                return this;

            var longest = knownDisplayTypes.Aggregate("", (best, x) => {
                if (displayType.StartsWith(x) && x.Length > best.Length) return x;
                return best;
            });

            if (string.IsNullOrEmpty(longest))
                return this;

            TemplateName += "." + longest;
            return this;
        }
    }
}