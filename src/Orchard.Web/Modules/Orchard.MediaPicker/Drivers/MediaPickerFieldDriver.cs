using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.MediaPicker.Fields;
using Orchard.MediaPicker.Settings;
using Orchard.Utility.Extensions;

namespace Orchard.MediaPicker.Drivers {
    public class MediaPickerFieldDriver : ContentFieldDriver<MediaPickerField> {

        public MediaPickerFieldDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(MediaPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(MediaPickerField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, MediaPickerField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_MediaPicker", GetDifferentiator(field, part),
                () => shapeHelper.Fields_MediaPicker());
        }

        protected override DriverResult Editor(ContentPart part, MediaPickerField field, dynamic shapeHelper) {
            return ContentShape("Fields_MediaPicker_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: "Fields/MediaPicker.Edit", Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, MediaPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            // if the model could not be bound, don't try to validate its properties
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<MediaPickerFieldSettings>();

                var extensions = String.IsNullOrWhiteSpace(settings.AllowedExtensions)
                        ? new string[0]
                        : settings.AllowedExtensions.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                if (extensions.Any() && field.Url != null && !extensions.Any(x => field.Url.EndsWith(x, StringComparison.OrdinalIgnoreCase))) {
                    updater.AddModelError("Url", T("The {0} field must have one of these extensions: {1}.", field.Name.CamelFriendly(), settings.AllowedExtensions));
                }

                if (settings.Required && String.IsNullOrWhiteSpace(field.Url)) {
                    updater.AddModelError("Url", T("The {0} field is required.", field.Name.CamelFriendly()));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, MediaPickerField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Url", value => field.Url = value);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "AlternateText", value => field.AlternateText = value);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Class", value => field.Class = value);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Style", value => field.Style = value);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Alignment", value => field.Alignment = value);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Width", value => field.Width = Int32.Parse(value));
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Height", value => field.Height = Int32.Parse(value));
        }

        protected override void Exporting(ContentPart part, MediaPickerField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Url", field.Url);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("AlternateText", field.AlternateText);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Class", field.Class);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Style", field.Style);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Alignment", field.Alignment);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Width", field.Width);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Height", field.Height);
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Url"), T("The url of the media."));
        }
    }
}