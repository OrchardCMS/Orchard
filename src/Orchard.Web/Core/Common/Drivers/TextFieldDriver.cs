using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Fields;
using Orchard.Core.ContentsLocation.Models;

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
            var location = field.GetLocation(displayType, "primary", "1");

            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part))
                .Location(location);
        }

        protected override DriverResult Editor(ContentPart part, TextField field) {
            var location = field.GetLocation("Editor", "primary", "1");

            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part))
                .Location(location);
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater) {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field);
        }
    }
}
