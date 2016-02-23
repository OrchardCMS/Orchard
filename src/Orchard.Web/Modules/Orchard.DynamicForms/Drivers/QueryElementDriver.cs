using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.DynamicForms.Elements;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tokens;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    [OrchardFeature("Orchard.DynamicForms.Projections")]
    public class QueryElementDriver : FormsElementDriver<Query> {
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;

        public QueryElementDriver(IFormsBasedElementServices formsServices, IProjectionManager projectionManager, IContentManager contentManager, ITokenizer tokenizer)
            : base(formsServices) {
            _projectionManager = projectionManager;
            _contentManager = contentManager;
            _tokenizer = tokenizer;
        }

        protected override EditorResult OnBuildEditor(Query element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var enumerationEditor = BuildForm(context, "QueryForm");
            var checkBoxValidation = BuildForm(context, "QueryValidation", "Validation:10");

            return Editor(context, autoLabelEditor, enumerationEditor, checkBoxValidation);
        }

        protected override void DescribeForm(DescribeContext context) {
            context.Form("QueryForm", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "QueryForm",
                    _OptionLabel: shape.Textbox(
                        Id: "OptionLabel",
                        Name: "OptionLabel",
                        Title: "Option Label",
                        Description: T("Optionally specify a label for the first option. If no label is specified, no empty option will be rendered."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _Query: shape.SelectList(
                        Id: "QueryId",
                        Name: "QueryId",
                        Title: "Query",
                        Description: T("Select the query to use as a source for the list.")),
                    _TextExpression: shape.Textbox(
                        Id: "TextExpression",
                        Name: "TextExpression",
                        Title: "Text Expression",
                        Value: "{Content.DisplayText}",
                        Description: T("Specify the expression to get the display text of each option."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _ValueExpression: shape.Textbox(
                        Id: "ValueExpression",
                        Name: "ValueExpression",
                        Title: "Value Expression",
                        Value: "{Content.Id}",
                        Description: T("Specify the expression to get the value of each option."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _DefaultValue: shape.Textbox(
                        Id: "DefaultValue",
                        Name: "DefaultValue",
                        Title: "Default Value",
                        Classes: new[] { "text", "large", "tokenized" },
                        Description: T("The default value of this query field.")),
                    _InputType: shape.SelectList(
                        Id: "InputType",
                        Name: "InputType",
                        Title: "Input Type",
                        Description: T("The control to render when presenting the list of options.")));

                // Query
                var queries = _contentManager.Query<QueryPart, QueryPartRecord>().Join<TitlePartRecord>().OrderBy(x => x.Title).List().ToArray();
                foreach (var query in queries) {
                    form._Query.Items.Add(new SelectListItem {Text = query.Name, Value = query.Id.ToString(CultureInfo.InvariantCulture)});
                }

                // Input Type
                form._InputType.Items.Add(new SelectListItem { Text = T("Select List").Text, Value = "SelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Multi Select List").Text, Value = "MultiSelectList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Radio List").Text, Value = "RadioList" });
                form._InputType.Items.Add(new SelectListItem { Text = T("Check List").Text, Value = "CheckList" });

                return form;
            });

            context.Form("QueryValidation", factory => {
                var shape = (dynamic)factory;
                var form = shape.Fieldset(
                    Id: "QueryValidation",
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

        protected override void OnDisplaying(Query element, ElementDisplayingContext context) {
            var queryId = element.QueryId;
            var typeName = element.GetType().Name;
            var displayType = context.DisplayType;
            var tokenData = context.GetTokenData();

            // Allow the initially selected value to be tokenized.
            // If a value was posted, use that value instead (without tokenizing it).
            if (element.PostedValue == null) {
                var defaultValue = _tokenizer.Replace(element.DefaultValue, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
                element.RuntimeValue = defaultValue;
            }

            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData);
            context.ElementShape.Options = GetOptions(element, context.DisplayType, queryId, tokenData).ToArray();
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, element.InputType));
        }

        private IEnumerable<SelectListItem> GetOptions(Query element, string displayType, int? queryId, IDictionary<string, object> tokenData) {
            var optionLabel = element.OptionLabel;
            var runtimeValues = GetRuntimeValues(element);

            if (!String.IsNullOrWhiteSpace(optionLabel)) {
                yield return new SelectListItem { Text = displayType != "Design" ? _tokenizer.Replace(optionLabel, tokenData) : optionLabel, Value = string.Empty };
            }

            if (queryId == null)
                yield break;

            var contentItems = _projectionManager.GetContentItems(queryId.Value).ToArray();
            var valueExpression = !String.IsNullOrWhiteSpace(element.ValueExpression) ? element.ValueExpression : "{Content.Id}";
            var textExpression = !String.IsNullOrWhiteSpace(element.TextExpression) ? element.TextExpression : "{Content.DisplayText}";
            
            foreach (var contentItem in contentItems) {
                var data = new {Content = contentItem};
                var value = _tokenizer.Replace(valueExpression, data);
                var text = _tokenizer.Replace(textExpression, data, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });

                yield return new SelectListItem {
                    Text = text,
                    Value = value,
                    Selected = runtimeValues.Contains(value, StringComparer.OrdinalIgnoreCase)
                };
            }
        }

        private IEnumerable<string> GetRuntimeValues(Query element) {
            var runtimeValue = element.RuntimeValue;
            return runtimeValue != null ? runtimeValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
        }
    }
}