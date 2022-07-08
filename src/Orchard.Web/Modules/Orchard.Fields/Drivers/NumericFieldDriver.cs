using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Fields.Fields;
using Orchard.Fields.Settings;
using Orchard.Fields.ViewModels;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Orchard.Fields.Drivers {
    public class NumericFieldDriver : ContentFieldDriver<NumericField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Numeric.Edit";
        private readonly Lazy<CultureInfo> _cultureInfo;

        public NumericFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;

            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(Services.WorkContext.CurrentCulture));
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
                return shapeHelper.Fields_Numeric()
                    .Settings(field.PartFieldDefinition.Settings.GetModel<NumericFieldSettings>())
                    .Value(Convert.ToString(field.Value, _cultureInfo.Value));
            });
        }

        protected override DriverResult Editor(ContentPart part, NumericField field, dynamic shapeHelper) {
            return ContentShape("Fields_Numeric_Edit", GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<NumericFieldSettings>();
                    var value = part.IsNew() && field.Value == null ? settings.DefaultValue : Convert.ToString(field.Value, _cultureInfo.Value);

                    var model = new NumericFieldViewModel {
                        Field = field,
                        Settings = settings,
                        Value = value
                    };

                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, NumericField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new NumericFieldViewModel();

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                Decimal value;

                var settings = field.PartFieldDefinition.Settings.GetModel<NumericFieldSettings>();

                field.Value = null;

                if (String.IsNullOrWhiteSpace(viewModel.Value)) {
                    if (settings.Required) {
                        updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                    }
                }
                else if (!Decimal.TryParse(viewModel.Value, NumberStyles.Any, _cultureInfo.Value, out value)) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid number", T(field.DisplayName)));
                }
                else {

                    field.Value = value;

                    if (settings.Minimum.HasValue && value < settings.Minimum.Value) {
                        updater.AddModelError(GetPrefix(field, part), T("The value must be greater than {0}", settings.Minimum.Value));
                    }

                    if (settings.Maximum.HasValue && value > settings.Maximum.Value) {
                        updater.AddModelError(GetPrefix(field, part), T("The value must be less than {0}", settings.Maximum.Value));
                    }

                    // checking the number of decimals
                    if (Math.Round(value, settings.Scale) != value) {
                        if (settings.Scale == 0) {
                            updater.AddModelError(GetPrefix(field, part), T("The field {0} must be an integer", T(field.DisplayName)));
                        }
                        else {
                            updater.AddModelError(GetPrefix(field, part), T("Invalid number of digits for {0}, max allowed: {1}", T(field.DisplayName), settings.Scale));
                        }
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
            context
                .Member(null, typeof(decimal), T("Value"), T("The value of the field."))
                .Enumerate<NumericField> (() => field => new[] { field.Value });
        }
    }
}
