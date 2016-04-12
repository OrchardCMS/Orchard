﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Fields.Drivers {
    public class EnumerationFieldDriver : ContentFieldDriver<EnumerationField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Enumeration.Edit";
        private readonly ITokenizer _tokenizer;

        public EnumerationFieldDriver(IOrchardServices services, ITokenizer tokenizer) {
            Services = services;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(EnumerationField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, EnumerationField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Enumeration", GetDifferentiator(field, part),
                                () => shapeHelper.Fields_Enumeration());
        }

        protected override DriverResult Editor(ContentPart part, EnumerationField field, dynamic shapeHelper) {
            return ContentShape("Fields_Enumeration_Edit", GetDifferentiator(field, part),
                () => {
                    if (field.Value == null) {
                        var settings = field.PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>();
                        if (!String.IsNullOrWhiteSpace(settings.DefaultValue)) {
                            field.Value = _tokenizer.Replace(settings.DefaultValue, new Dictionary<string, object> { { "Content", part.ContentItem } });
                        }
                    }

                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, EnumerationField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>();

                if (field.SelectedValues.Length == 0 && !String.IsNullOrWhiteSpace(settings.DefaultValue) && !String.IsNullOrWhiteSpace(settings.Options)) {
                    field.Value = _tokenizer.Replace(settings.DefaultValue, new Dictionary<string, object> { { "Content", part.ContentItem } });

                    string[] options = settings.Options.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var selectedValues = field.SelectedValues.ToList();
                    selectedValues.RemoveAll(value => !options.Any(value.Equals));
                    field.SelectedValues = selectedValues.ToArray();
                }

                if (settings.Required && field.SelectedValues.Length == 0) {
                    updater.AddModelError(field.Name, T("The field {0} is mandatory", T(field.DisplayName)));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, EnumerationField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Value", v => field.Value = v);
        }

        protected override void Exporting(ContentPart part, EnumerationField field, ExportContentContext context) {
            if (!String.IsNullOrEmpty(field.Value))
                context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Value", field.Value);
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The selected values of the field."))
                .Enumerate<EnumerationField>(() => field => field.SelectedValues);
        }
    }
}
