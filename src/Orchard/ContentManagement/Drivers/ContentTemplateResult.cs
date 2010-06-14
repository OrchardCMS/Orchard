using System.Linq;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class ContentTemplateResult : DriverResult {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }

        public ContentTemplateResult(object model, string templateName, string prefix) {
            Model = model;
            TemplateName = templateName;
            Prefix = prefix;
        }

        public override void Apply(BuildDisplayModelContext context) {
            context.ViewModel.Zones.AddDisplayPart(
                Zone + ":" + Position, Model, TemplateName, Prefix);
        }

        public override void Apply(BuildEditorModelContext context) {
            context.ViewModel.Zones.AddEditorPart(
                Zone + ":" + Position, Model, TemplateName, Prefix);
        }

        public ContentTemplateResult Location(string zone) {
            Zone = zone;
            return this;
        }

        public ContentTemplateResult Location(string zone, string position) {
            Zone = zone;
            Position = position;
            return this;
        }

        public ContentTemplateResult LongestMatch(string displayType, params string[] knownDisplayTypes) {

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