using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization;

namespace Orchard.Fields.Drivers {
    public class LinkFieldDriver : ContentFieldDriver<LinkField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Link.Edit";

        public LinkFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(ContentField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, LinkField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Link", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();
                return shapeHelper.Fields_Link().Settings(settings);
            });
        }

        protected override DriverResult Editor(ContentPart part, LinkField field, dynamic shapeHelper) {
            return ContentShape("Fields_Link_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, LinkField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();
                if (settings.Required && string.IsNullOrWhiteSpace(field.Value)) {
                    updater.AddModelError(GetPrefix(field, part), T("Url is required for {0}", field.DisplayName));
                }
                else if (!string.IsNullOrWhiteSpace(field.Value) && !Uri.IsWellFormedUriString(field.Value, UriKind.RelativeOrAbsolute)) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid url.", field.Value));
                }
                else if (settings.LinkTextMode == LinkTextMode.Required && string.IsNullOrWhiteSpace(field.Text)) {
                    updater.AddModelError(GetPrefix(field, part), T("Text is required for {0}.", field.DisplayName));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, LinkField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Text", v => field.Text = v);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Url", v => field.Value = v);
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Target", v => field.Target = v);
        }

        protected override void Exporting(ContentPart part, LinkField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Text", field.Text);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Url", field.Value);
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Target", field.Target);
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member("Text", typeof(string), T("Text"), T("The text of the link."))
                .Member(null, typeof(string), T("Url"), T("The url of the link."));
        }
    }
}
