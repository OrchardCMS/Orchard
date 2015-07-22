using System;
using System.Globalization;
using Orchard.DynamicForms.Elements;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    public class NumFieldElementDriver : FormsElementDriver<NumField>{
        private readonly ITokenizer _tokenizer;
        private readonly ICultureAccessor _cultureAccessor;

        public NumFieldElementDriver(IFormManager formManager, ITokenizer tokenizer, ICultureAccessor cultureAccessor) : base(formManager) {
            _tokenizer = tokenizer;
            _cultureAccessor = cultureAccessor;
        }

        protected override EditorResult OnBuildEditor(NumField element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var numFieldEditor = BuildForm(context, "NumField");
            var numFieldValidation = BuildForm(context, "NumFieldValidation", "Validation:10");

            if (context.Updater != null) {
                var model = new NumFieldNumericModel { };
                if (context.Updater.TryUpdateModel(model, null, new string[] { "Min", "Max", "Scale", "Value" }, null)) {
                    element.Data["Min"] = model.Max != null ? model.Min.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                    element.Data["Max"] = model.Max != null ? model.Max.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                    element.Data["Scale"] = model.Scale != null ? model.Scale.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                    element.Data["Value"] = model.Value != null ? model.Value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                }
            }
            else {
                Decimal value;
                if (Decimal.TryParse(numFieldValidation._Min.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                    numFieldValidation._Min.Value(value.ToString(_cultureAccessor.CurrentCulture));
                }
                if (Decimal.TryParse(numFieldValidation._Max.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                    numFieldValidation._Max.Value(value.ToString(_cultureAccessor.CurrentCulture));
                }
                Int32 scale;
                if (Int32.TryParse(numFieldValidation._Scale.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out scale)) {
                    numFieldValidation._Scale.Value(scale.ToString(_cultureAccessor.CurrentCulture));
                }
                if (Decimal.TryParse(numFieldEditor._Value.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                    numFieldEditor._Value.Value(value.ToString(_cultureAccessor.CurrentCulture));
                }
            }

            return Editor(context, autoLabelEditor, numFieldEditor, numFieldValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("NumField", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "NumField",
                    _Value: shape.Textbox(
                        Id: "Value",
                        Name: "Value",
                        Title: "Value",
                        Classes: new[] { "medium" },
                        Description: T("The value of this num field.")));

                return form;
            });

            context.Form("NumFieldValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "NumFieldValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "IsRequired",
                        Name: "IsRequired",
                        Title: "Required",
                        Value: "true",
                        Description: T("Check to make this num field a required field.")),
                    _Min: shape.Textbox(
                        Id: "Min",
                        Name: "Min",
                        Title: "Minimum value",
                        Classes: new[] { "medium" },
                        Description: T("The minimum value required.")),
                    _Max: shape.Textbox(
                        Id: "Max",
                        Name: "Max",
                        Title: "Maximum value",
                        Classes: new[] { "medium" },
                        Description: T("The maximum value allowed.")),
                    _Scale: shape.Textbox(
                        Id: "Scale",
                        Name: "Scale",
                        Title: "Number of decimals",
                        Classes: new[] { "medium" },
                        Description: T("The maximum number of decimals allowed.")),
                    _CustomValidationMessage: shape.Textbox(
                        Id: "CustomValidationMessage",
                        Name: "CustomValidationMessage",
                        Title: "Custom Validation Message",
                        Classes: new[] { "large", "tokenized" },
                        Description: T("Optionally provide a custom validation message.")),
                    _ShowValidationMessage: shape.Checkbox(
                        Id: "ShowValidationMessage",
                        Name: "ShowValidationMessage",
                        Title: "Show Validation Message",
                        Value: "true",
                        Description: T("Autogenerate a validation message when a validation error occurs for the current field. Alternatively, to control the placement of the validation message you can use the ValidationMessage element instead.")));

                return form;
            });
        }

        protected override void OnDisplaying(NumField element, ElementDisplayingContext context) {
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, context.GetTokenData());
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, context.GetTokenData());

            Decimal value;
            var processedValue = element.RuntimeValue;
            if (element.PostedValue == null && Decimal.TryParse(processedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                processedValue = value.ToString(_cultureAccessor.CurrentCulture);
            }
            context.ElementShape.ProcessedValue = processedValue;
        }
    }
}