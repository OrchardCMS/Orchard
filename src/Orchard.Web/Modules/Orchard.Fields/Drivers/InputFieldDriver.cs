﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization;
using Orchard.Tokens;
using System;
using System.Collections.Generic;

namespace Orchard.Fields.Drivers {
    public class InputFieldDriver : ContentFieldDriver<InputField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Input.Edit";
        private readonly ITokenizer _tokenizer;

        public InputFieldDriver(IOrchardServices services, ITokenizer tokenizer) {
            Services = services;
            _tokenizer = tokenizer;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(InputField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, InputField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Input", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<InputFieldSettings>();
                return shapeHelper.Fields_Input().Settings(settings);
            });
        }

        protected override DriverResult Editor(ContentPart part, InputField field, dynamic shapeHelper) {
            return ContentShape("Fields_Input_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, InputField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<InputFieldSettings>();

                if (String.IsNullOrWhiteSpace(field.Value) && !String.IsNullOrWhiteSpace(settings.DefaultValue)) {
                     field.Value = _tokenizer.Replace(settings.DefaultValue, new Dictionary<string, object> { { "Content", part.ContentItem } });
                }

                if (settings.Required && String.IsNullOrWhiteSpace(field.Value)) {
                    updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, InputField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Value", v => field.Value = v);
        }

        protected override void Exporting(ContentPart part, InputField field, ExportContentContext context) {
            if (!String.IsNullOrEmpty(field.Value))
                context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Value", field.Value);
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), T("Value"), T("The value of the field."))
                .Enumerate<InputField>(() => field => new[] { field.Value });
        }
    }
}
