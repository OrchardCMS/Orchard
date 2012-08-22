using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Localization;

namespace Orchard.Fields.Drivers {
    public class NumericFieldDriver : ContentFieldDriver<NumericField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Numeric.Edit";

        public NumericFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(NumericField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, NumericField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_Numeric", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<NumericFieldSettings>();
                return shapeHelper.Fields_Numeric().Settings(settings);
            });
        }

        protected override DriverResult Editor(ContentPart part, NumericField field, dynamic shapeHelper) {
            return ContentShape("Fields_Numeric_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: field, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, NumericField field, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(field, GetPrefix(field, part), null, null)) {
                var settings = field.PartFieldDefinition.Settings.GetModel<NumericFieldSettings>();

                if (settings.Required && !field.Value.HasValue) {
                    updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                }

                if (settings.Minimum.HasValue && field.Value.HasValue && field.Value.Value < settings.Minimum.Value) {
                    updater.AddModelError(GetPrefix(field, part), T("The value must be greater than {0}", settings.Minimum.Value));
                }

                if (settings.Maximum.HasValue && field.Value.HasValue && field.Value.Value > settings.Maximum.Value) {
                    updater.AddModelError(GetPrefix(field, part), T("The value must be less than {0}", settings.Maximum.Value));
                }

                // checking the number of decimals
                if(field.Value.HasValue && Math.Round(field.Value.Value, settings.Scale) != field.Value.Value) {
                    if(settings.Scale == 0) {
                        updater.AddModelError(GetPrefix(field, part), T("The field {0} must be an integer", field.DisplayName));    
                    }
                    else {
                        updater.AddModelError(GetPrefix(field, part), T("Invalid number of digits for {0}, max allowed: {1}", field.DisplayName, settings.Scale));
                    }
                    
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, NumericField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Value", v => field.Value = decimal.Parse(v, CultureInfo.InvariantCulture), () => field.Value = (decimal?)null);
        }

        protected override void Exporting(ContentPart part, NumericField field, ExportContentContext context) {
            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Value", !field.Value.HasValue ? String.Empty : field.Value.Value.ToString(CultureInfo.InvariantCulture));
        }

        protected override void Describe(DescribeMembersContext context) {
            context.Member(null, typeof(decimal), T("Value"), T("The value of the field."));
        }
    }
}
