using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.Environment.Extensions;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.Services;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Tokens;
using Orchard.DynamicForms.Helpers;
using DescribeContext = Orchard.Forms.Services.DescribeContext;
using Orchard.ContentManagement;
using Orchard.Conditions.Services;
using Orchard.DynamicForms.Services.Models;

namespace Orchard.DynamicForms.Drivers {
    [OrchardFeature("Orchard.DynamicForms.Taxonomies")]
    public class TaxonomyElementDriver : FormsElementDriver<Taxonomy> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITokenizer _tokenizer;
        private readonly IFormService _formService;
        private readonly IContentManager _contentManager;

        public TaxonomyElementDriver(IFormsBasedElementServices formsServices, IConditionManager conditionManager, ITaxonomyService taxonomyService, ITokenizer tokenizer, IFormService formService, IContentManager contentManager)
            : base(formsServices, conditionManager, tokenizer) {
            _taxonomyService = taxonomyService;
            _tokenizer = tokenizer;
            _formService = formService;
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(Taxonomy element, ElementEditorContext context) {
            var autoLabelEditor = BuildForm(context, "AutoLabel");
            var editableEditor = BuildForm(context, "Editable");
            var enumerationEditor = BuildForm(context, "TaxonomyForm");
            var checkBoxValidation = BuildForm(context, "TaxonomyValidation", "Validation:10");

            return Editor(context, autoLabelEditor, editableEditor, enumerationEditor, checkBoxValidation);
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
                    _ParentTaxonomyElementName: shape.Textbox(
                        Id: "ParentTaxonomyElementName",
                        Name: "ParentTaxonomyElementName",
                        Title: "Parent Taxonomy Element Name",
                        Classes: new[] { "text" },
                        Description: T("The name of the parent Taxonomy Element to get childent taxonomy terms to render. An empty value means that root term of the taxonomy will be used.")),
                    _LevelsToRender: shape.Textbox(
                        Id: "LevelsToRender",
                        Name: "LevelsToRender",
                        Title: "Number of Levels to render",
                        Classes: new[] { "text", "small" },
                        Description: T("Select the numbers of levels to render. A 0 value means all the levels will be rendered.")),
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

        protected override void OnDisplaying(Taxonomy element, ElementDisplayingContext context) {
            var taxonomyId = element.TaxonomyId;
            var typeName = element.GetType().Name;
            var displayType = context.DisplayType;
            var tokenData = context.GetTokenData();
            
            context.ElementShape.ProcessedName = _tokenizer.Replace(element.Name, tokenData);
            if (!string.IsNullOrWhiteSpace(element.ParentTaxonomyElementName) && element.Form != null ) {
                var parentElement = _formService.GetFormElements(element.Form).FirstOrDefault(e => e.Name == element.ParentTaxonomyElementName) as Taxonomy;
                if (parentElement != null) {                    
                    context.ElementShape.ParentTaxonomyElementName = element.ParentTaxonomyElementName;
                    context.ElementShape.ParentTaxonomyElementInputType = parentElement.InputType;
                }
            }
            context.ElementShape.ProcessedLabel = _tokenizer.Replace(element.Label, tokenData, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
            context.ElementShape.TermOptions = GetTermOptions(element, context.DisplayType, taxonomyId, tokenData).ToArray();
            context.ElementShape.Disabled = ((context.DisplayType != "Design") && !String.IsNullOrWhiteSpace(element.ReadOnlyRule) && EvaluateRule(element.ReadOnlyRule, new { Element = element }));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}__{1}", typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Elements_{0}_{1}__{2}", typeName, displayType, element.InputType));
        }

        protected override void OnExporting(Taxonomy element, ExportElementContext context) {
            var taxonomy = element.TaxonomyId != null ? _contentManager.Get<TaxonomyPart>(element.TaxonomyId.Value) : default(TaxonomyPart);
            var taxonomyIdentity = taxonomy != null ? _contentManager.GetItemMetadata(taxonomy).Identity.ToString() : default(string);

            if (taxonomyIdentity != null)
                context.ExportableData["TaxonomyId"] = taxonomyIdentity;
        }

        protected override void OnImportCompleted(Taxonomy element, ImportElementContext context) {
            var taxonomyIdentity = context.ExportableData.Get("TaxonomyId");
            var taxonomy = taxonomyIdentity != null ? context.Session.GetItemFromSession(taxonomyIdentity) : default(ContentManagement.ContentItem);
            
            if (taxonomy == null)
                return;
            
            element.TaxonomyId = taxonomy.Id;
        }
        
        private IEnumerable<SelectListItem> GetTermOptions(Taxonomy element, string displayType, int? taxonomyId, IDictionary<string, object> tokenData) {
            var optionLabel = element.OptionLabel;
            var runtimeValues = GetRuntimeValues(element);

            if (!String.IsNullOrWhiteSpace(optionLabel)) {
                yield return new SelectListItem { Text = displayType != "Design" ? _tokenizer.Replace(optionLabel, tokenData) : optionLabel, Value = string.Empty };
            }

            if (taxonomyId == null)
                yield break;

            Taxonomy[] taxonomyElements;
            if (element.Form == null)
                taxonomyElements = new Taxonomy[1] { element };
            else
                taxonomyElements = _formService.GetFormElements(element.Form).Select(e => e as Taxonomy).Where(e => e != null).ToArray();
            var termIds = GetRuntimeTermIds(element,taxonomyElements);
            
            IEnumerable<TermPart> parentTerms = null;
            if (termIds.Any()) {
                var levelsToSkip = GetTaxonomyLevelsToSkip(element, taxonomyElements);

                IEnumerable<TermPart> selectedTerms = _contentManager.GetMany<TermPart>(termIds.Select(s=>int.Parse(s)),VersionOptions.Published,QueryHints.Empty).ToArray();

                IEnumerable<TermPart> selectedPartialTerms = selectedTerms.Where(t => t.Level > levelsToSkip + 1)
                    .Select(t => _taxonomyService.GetTerm(int.Parse(t.FullPath.Split('/').Skip(levelsToSkip + 1).First()))).ToArray();
                runtimeValues = runtimeValues.Union(selectedPartialTerms.Select(t=>t.Id.ToString()));

                parentTerms = selectedTerms.Where(t => levelsToSkip > 0 && t.Level > levelsToSkip)
                    .Select(t => int.Parse(t.FullPath.Split('/').Skip(levelsToSkip).First())).Distinct()
                    .Select(id => _taxonomyService.GetTerm(id)).ToArray();      
            }

            IEnumerable<TermPart> terms = null;
            if (string.IsNullOrWhiteSpace(element.ParentTaxonomyElementName))
                terms = _taxonomyService.GetTerms(taxonomyId.Value, element.LevelsToRender.GetValueOrDefault());
            else if (parentTerms != null) 
                terms = parentTerms.SelectMany(t=>_taxonomyService.GetChildren(t, false, element.LevelsToRender.GetValueOrDefault()));            
            else
                yield break;

            IEnumerable<SelectListItem> projection = terms.GetSelectListItems(element,_tokenizer, runtimeValues);

            foreach (var item in projection) {
                yield return item;
            }
        }

        private IEnumerable<string> GetRuntimeTermIds(Taxonomy element, IEnumerable<Taxonomy> taxonomyElements) {
            Taxonomy childElement = element;
            IEnumerable<string> termIds = new string[0];
            var remainingTaxonomyElements = taxonomyElements.Where(t => !string.IsNullOrWhiteSpace(t.ParentTaxonomyElementName)).ToDictionary(t=>t.ParentTaxonomyElementName);
            do {
                termIds = GetRuntimeValues(childElement);
                if (!termIds.Any()) {
                    var currentElementName = childElement.Name;
                    if (remainingTaxonomyElements.TryGetValue(currentElementName, out childElement))
                        remainingTaxonomyElements.Remove(currentElementName);
                    else
                        return termIds;
                }
            } while (!termIds.Any() && childElement != null);

            return termIds;
        }

        private int GetTaxonomyLevelsToSkip(Taxonomy element, IEnumerable<Taxonomy> taxonomyElements) {
            var levelsToSkip = 0;          
            var parentFormName = element.ParentTaxonomyElementName;
            Taxonomy parentElement;
            do {
                parentElement = taxonomyElements.FirstOrDefault(e => e.Name == parentFormName);
                parentFormName = null;
                if (parentElement != null && parentElement.LevelsToRender.GetValueOrDefault() > 0) {
                    levelsToSkip += parentElement.LevelsToRender.Value;
                    parentFormName = parentElement.ParentTaxonomyElementName;
                }
            } while (parentFormName != null && parentElement.LevelsToRender.GetValueOrDefault() > 0);
            return levelsToSkip;
        }

        private IEnumerable<string> GetRuntimeValues(Taxonomy element) {
            var runtimeValue = element.RuntimeValue;
            return runtimeValue != null ? runtimeValue.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries) : Enumerable.Empty<string>();
        }
    }
}