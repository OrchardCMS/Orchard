using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Drivers {
    public class PartTemplateResult : DriverResult {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }

        public PartTemplateResult(object model, string templateName, string prefix) {
            Model = model;
            TemplateName = templateName;
            Prefix = prefix;
        }

        public override void Apply(BuildDisplayModelContext context) {
            context.DisplayModel.Zones.AddDisplayPart(
                Zone + ":" + Position, Model, TemplateName, Prefix);
        }

        public override void Apply(BuildEditorModelContext context) {
            context.EditorModel.Zones.AddEditorPart(
                Zone + ":" + Position, Model, TemplateName, Prefix);
        }

        public PartTemplateResult Location(string zone) {
            Zone = zone;
            return this;
        }

        public PartTemplateResult Location(string zone, string position) {
            Zone = zone;
            Position = position;
            return this;
        }
    }
}