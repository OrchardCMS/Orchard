﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Settings;
using Orchard.Fields.Fields;
using Orchard.Localization;
using System;

namespace Orchard.Fields.Drivers {
    public class EnumerationFieldDriver : ContentFieldDriver<EnumerationField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Enumeration.Edit";

        public EnumerationFieldDriver(IOrchardServices services) {
            Services = services;
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
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, EnumerationField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<EnumerationFieldSettings>();
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
