using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentManagement.Drivers {
    public class TemplateResult : DriverResult {
        public object Model { get; set; }
        public string TemplateName { get; set; }
        public string Prefix { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }

        public TemplateResult(object model, string templateName, string prefix) {
            Model = model;
            TemplateName = templateName;
            Prefix = prefix;
        }

        public override void Apply(BuildDisplayModelContext context) {
            context.AddDisplay(new TemplateViewModel(Model, Prefix) {
                                                                        TemplateName = TemplateName,
                                                                        ZoneName = Zone,
                                                                        Position = Position
                                                                    });
        }

        public override void Apply(BuildEditorModelContext context) {
            context.AddEditor(new TemplateViewModel(Model, Prefix) {
                                                                       TemplateName = TemplateName,
                                                                       ZoneName = Zone,
                                                                       Position = Position
                                                                   });
        }

        public TemplateResult Location(string zone) {
            Zone = zone;
            return this;
        }

        public TemplateResult Location(string zone, string position) {
            Zone = zone;
            Position = position;
            return this;
        }
    }
}