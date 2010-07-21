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
            var locationSettings = field.PartFieldDefinition.Settings.GetModel<LocationSettings>().Get(displayType, "primary", "5");

            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part))
                .Location(locationSettings.Zone, locationSettings.Position);
        }

        protected override DriverResult Editor(ContentPart part, TextField field) {
            var locationSettings = field.PartFieldDefinition.Settings.GetModel<LocationSettings>().Get("Editor", "primary", "5");

            return ContentFieldTemplate(field, TemplateName, GetPrefix(field, part))
                .Location(locationSettings.Zone, locationSettings.Position);
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater) {
            updater.TryUpdateModel(field, GetPrefix(field, part), null, null);
            return Editor(part, field);
        }
    }
}
