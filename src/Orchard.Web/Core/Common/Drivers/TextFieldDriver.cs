using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class TextFieldDriver : ContentFieldDriver<TextField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Common.TextField";

        public TextFieldDriver(IOrchardServices services) {
            Services = services;
        }
        
        private static string GetPrefix(TextField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        protected override DriverResult Display(ContentPart part, TextField field, string displayType) {
            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part));
        }

        protected override DriverResult Editor(ContentPart part, TextField field) {
            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part)).Location("primary", "5");
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater) {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field);
        }

    }
}
