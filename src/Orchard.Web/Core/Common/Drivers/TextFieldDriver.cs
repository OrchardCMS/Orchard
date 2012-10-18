using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.Core.Common.Drivers {
    [UsedImplicitly]
    public class TextFieldDriver : ContentFieldDriver<TextField> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public TextFieldDriver(IOrchardServices services, IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
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

                    object fieldValue = new HtmlString(_htmlFilters.Aggregate(field.Value, (text, filter) => filter.ProcessContent(text, settings.Flavor)));
                    return shapeHelper.Fields_Common_Text(Name: field.Name, Value: fieldValue);
                });
        }

        protected override DriverResult Editor(ContentPart part, TextField field, dynamic shapeHelper) {
            return ContentShape("Fields_Common_Text_Edit", GetDifferentiator(field, part),
                () => {
                    var viewModel = new TextFieldDriverViewModel {
                        Field = field,
                        Text = field.Value,
                        Settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>()
                    };

                    return shapeHelper.EditorTemplate(TemplateName: "Fields.Common.Text.Edit", Model: viewModel, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, TextField field, IUpdateModel updater, dynamic shapeHelper) {
            
            if(field.Name == "Note") {
                throw new ArgumentException();
            }

            var viewModel = new TextFieldDriverViewModel {
                Field = field,
                Text = field.Value,
                Settings = field.PartFieldDefinition.Settings.GetModel<TextFieldSettings>()
            };

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                if (viewModel.Settings.Required && string.IsNullOrWhiteSpace(viewModel.Text)) {
                    updater.AddModelError("Text", T("The fields {0} is mandatory", field.DisplayName));
                    return ContentShape("Fields_Common_Text_Edit", GetDifferentiator(field, part),
                                        () => shapeHelper.EditorTemplate(TemplateName: "Fields.Common.Text.Edit", Model: viewModel, Prefix: GetPrefix(field, part)));
                }

                field.Value = viewModel.Text;
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

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The text associated with the field."));
        }
    }
}