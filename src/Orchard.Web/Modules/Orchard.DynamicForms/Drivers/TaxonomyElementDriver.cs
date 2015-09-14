using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Taxonomies.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    [OrchardFeature("Orchard.DynamicForms.Taxonomies")]
    public class TaxonomyElementDriver : FormsElementDriver<Taxonomy> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITokenizer _tokenizer;

        public TaxonomyElementDriver(IFormManager formManager, ITaxonomyService taxonomyService, ITokenizer tokenizer)
            : base(formManager) {
            _taxonomyService = taxonomyService;
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(Taxonomy element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var enumerationEditor = BuildForm(context, "TaxonomyForm");
            var checkBoxValidation = BuildForm(context, "TaxonomyValidation", "Validation:10");

            return Editor(context, autoLabelEditor, enumerationEditor, checkBoxValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("TaxonomyForm", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TaxonomyForm",
                    _OptionLabel: shape.Textbox(
                        Id: "OptionLabel",
                        Name: "OptionLabel",
                        Title: "Option Label",
                        Description: T("Optionally specify a label for the first option. If no label is specified, no empty option will be rendered.")),
                    _Taxonomy: shape.SelectList(
                        Id: "TaxonomyId",
                        Name: "TaxonomyId",
                        Title: "Taxonomy",
                        Description: T("Select the taxonomy to use as a source for the list.")),
                    _SortOrder: shape.SelectList(
                        Id: "SortOrder",
                        Name: "SortOrder",
                        Title: "Sort Order",
                        Description: T("The sort order to use when presenting the term values.")),
                    _TextExpression: shape.Textbox(
                        Id: "TextExpression",
                        Name: "TextExpression",
                        Title: "Text Expression",
                        Value: "{Content.DisplayText}",
                        Description: T("Specify the expression to get the display text of each option."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _ValueExpression: shape.Textbox(
                        Id: "ValueExpression",
                        Name: "ValueExpression",
                        Title: "Value Expression",
                        Value: "{Content.Id}",
                        Description: T("Specify the expression to get the value of each option."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _InputType: shape.SelectList(
                        Id: "InputType",
                        Name: "InputType",
                        Title: "Input Type",
                        Description: T("The control to render when presenting the list of options.")));

                // Taxonomy
                var taxonomies = _taxonomyService.GetTaxonomies();
                foreach (var taxonomy in taxonomies) {
                    form._Taxonomy.Items.Add(new SelectListItem { Text = taxonomy.Name, Value = taxonomy.Id.ToString(CultureInfo.InvariantCulture) });
                }

                // Sort Order
                form._SortOrder.Items.Add(new SelectListItem { Text = T("None").Text, Value = "" });
                form._SortOrder.Items.Add(new SelectListItem { Text = T("Ascending").Text, Value = "Asc" });
                form._SortOrder.Items.Add(new SelectListItem { Text = T("Descending").Text, Value = "Desc" });

                // Input Type
                form._InputType.Items.Add(new SelectListItem { Text = T("Select List").Text, Value = "SelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Multi Select List").Text, Value = "MultiSelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Radio List").Text, Value = "RadioList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Check List").Text, Value = "CheckList" });

                return form;
            });

            context.Form("TaxonomyValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "TaxonomyValidation",
                    _IsRequired: shape.Checkbox(
                        Id: "Required",
                        Name: "Required",
                        Title: "Required",
                        Value: "true",
                        Description: T("Tick this checkbox to make at least one option required.")),
                    _CustomValidationMessage: shape.Textbox(
                        Id: "CustomValidationMessage",
                        Name: "CustomValidationMessage",
                        Title: "Custom Validation Message",
                        Classes: new[] { "text", "large", "tokenized" },
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

        protected override void OnDisplaying(Taxonomy element, ElementDisplayContext context) {
            var taxonomyId = element.TaxonomyId;
            var typeName = element.GetType().Name;
            var displayType = context.DisplayType;
            var tokenData = context.GetTokenData();

            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData);
            context.ElementShape.TermOptions = GetTermOptions(element, context.DisplayType, taxonomyId, tokenData).ToArray();
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, element.InputType));
        }

        private IEnumerable<SelectListItem> GetTermOptions(Taxonomy element, string displayType, int? taxonomyId, IDictionary<string, object> tokenData) {
            var optionLabel = element.OptionLabel;
            var runtimeValues = GetRuntimeValues(element);

            if (!String.IsNullOrWhiteSpace(optionLabel)) {
                yield return new SelectListItem { Text = displayType != "Design" ? _tokenizer.Replace(optionLabel, tokenData) : optionLabel, Value = string.Empty };
            }

            if (taxonomyId == null)
                yield break;

            var terms = _taxonomyService.GetTerms(taxonomyId.Value);
            var valueExpression = !String.IsNullOrWhiteSpace(element.ValueExpression) ? element.ValueExpression : "{Content.Id}";
            var textExpression = !String.IsNullOrWhiteSpace(element.TextExpression) ? element.TextExpression : "{Content.DisplayText}";

            var projection = terms.Select(x => {
                var data = new {Content = x};
                var value = _tokenizer.Replace(valueExpression, data);
                var text = _tokenizer.Replace(textExpression, data);

                return new SelectListItem {
                    Text = text,
                    Value = value,
                    Selected = runtimeValues.Contains(value, StringComparer.OrdinalIgnoreCase)
                };
            });

            switch (element.SortOrder) {
                case "Asc":
                    projection = projection.OrderBy(x => x.Text);
                    break;
                case "Desc":
                    projection = projection.OrderByDescending(x => x.Text);
                    break;
            }

            foreach (var item in projection) {
                yield return item;
            }
        }

        private IEnumerable<string> GetRuntimeValues(Taxonomy element) {
            var runtimeValue = element.RuntimeValue;
            return runtimeValue != null ? runtimeValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
        }
    }
}