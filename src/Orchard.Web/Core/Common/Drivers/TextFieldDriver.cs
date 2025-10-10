using System;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Common.Drivers {
    public class TextFieldDriver : ContentFieldDriver<TextField> {
        private readonly IHtmlFilterProcessor _htmlFilterProcessor;

        public TextFieldDriver(IOrchardServices services, IHtmlFilterProcessor htmlFilterProcessor) {
            _htmlFilterProcessor = htmlFilterProcessor;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        private static string GetPrefix(TextField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private string GetDifferentiator(TextField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, TextField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Common_Text", GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>();
                    var fieldValue = new HtmlString(_htmlFilterProcessor.ProcessFilters(field.Value, settings.Flavor, part));
                    return shapeHelper.Fields_Common_Text(Name: field.Name, Value: fieldValue);
                });
        }

        protected override DriverResult Editor(ContentPart part, TextField field, dynamic shapeHelper) {
            return ContentShape("Fields_Common_Text_Edit", GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>();
                    var text = part.IsNew() && String.IsNullOrEmpty(field.Value) ? settings.DefaultValue : field.Value;

                    var viewModel = new TextFieldDriverViewModel {
                        Field = field,
                        Text = text,
                        Settings = settings,
                        ContentItem = part.ContentItem
                    };

                    return shapeHelper.EditorTemplate(TemplateName: "Fields.Common.Text.Edit", Model: viewModel, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater, dynamic shapeHelper) {

            var viewModel = new TextFieldDriverViewModel();

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>();

                field.Value = viewModel.Text;

                if (settings.Required && String.IsNullOrWhiteSpace(field.Value)) {
                    updater.AddModelError("Text", T("The {0} field is required.", T(field.DisplayName)));
                }

                if (settings.MaxLength > 0) {

                    var value = new HtmlString(_htmlFilterProcessor.ProcessFilters(field.Value, settings.Flavor, part))
                        .ToString().RemoveTags();

                    if (value.Length > settings.MaxLength) {
                        updater.AddModelError("Text", T("The maximum allowed length for the field {0} is {1}", T(field.DisplayName), settings.MaxLength));
                    }
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, TextField field, ImportContentContext context) {
            var importedText = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "Text");
            if (importedText != null) {
                field.Value = importedText;
            }
        }

        protected override void Exporting(ContentPart part, TextField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Text", field.Value);
        }

        protected override void Cloning(ContentPart part, TextField originalField, TextField cloneField, CloneContentContext context) {
            cloneField.Value = originalField.Value;
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The text associated with the field."))
                .Enumerate<TextField>(() => field => new[] { field.Value });
        }
    }
}
