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
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Utility.Extensions;
using DescribeContext = Orchard.Forms.Services.DescribeContext;

namespace Orchard.DynamicForms.Drivers {
    [OrchardFeature("Orchard.DynamicForms.Taxonomies")]
    public class TaxonomyDriver : FormsElementDriver<Taxonomy> {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyDriver(IFormManager formManager, ITaxonomyService taxonomyService)
            : base(formManager) {
            _taxonomyService = taxonomyService;
        }

        protected override IEnumerable<string> FormNames {
            get {
                yield return "AutoLabel";
                yield return "TaxonomyForm";
            }
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
                    _ValueType_TermText: shape.Radio(
                        Id: "ValueType_TermText",
                        Name: "ValueType",
                        Title: "Use Term Name",
                        Checked: true,
                        Value: "TermText",
                        Description: T("Select this option to use the term name as the option value.")),
                    _ValueType_TermId: shape.Radio(
                        Id: "ValueType_TermId",
                        Name: "ValueType",
                        Title: "Use Term ID",
                        Checked: false,
                        Value: "TermId",
                        Description: T("Select this option to use the term ID as the option value.")),
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
        }

        protected override void OnDisplaying(Taxonomy element, ElementDisplayContext context) {
            var taxonomyId = element.TaxonomyId;
            var typeName = element.GetType().Name;
            var category = element.Category.ToSafeName();
            var displayType = context.DisplayType;

            context.ElementShape.TermOptions = GetTermOptions(element, taxonomyId).ToArray();
            context.ElementShape.Metadata.Alternates.Add(String.Format("Element__{0}__{1}__{2}", category, typeName, element.InputType));
            context.ElementShape.Metadata.Alternates.Add(String.Format("Element_{0}__{1}__{2}__{3}", displayType, category, typeName, element.InputType));
        }

        private IEnumerable<SelectListItem> GetTermOptions(Taxonomy element, int? taxonomyId) {
            var optionLabel = element.OptionLabel;

            if (!String.IsNullOrWhiteSpace(optionLabel)) {
                yield return new SelectListItem { Text = optionLabel };
            }

            if (taxonomyId == null)
                yield break;

            var terms = _taxonomyService.GetTerms(taxonomyId.Value);
            var valueAccessor = element.UseTermId ? (Func<TermPart, string>)(x => x.Id.ToString()) : (x => x.Name);
            var projection = terms.Select(x => new SelectListItem {Text = x.Name, Value = valueAccessor(x)});

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
    }
}