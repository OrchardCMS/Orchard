using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Drivers {
    public class TaxonomyNavigationPartDriver : ContentPartDriver<TaxonomyNavigationPart> {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyNavigationPartDriver(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        protected override string Prefix { get { return "TaxonomyNavigationPart"; } }

        protected override DriverResult Editor(TaxonomyNavigationPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TaxonomyNavigationPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Navigation_Taxonomy_Edit", () => {
                    var model = new TaxonomyNavigationViewModel {
                        SelectedTaxonomyId = part.TaxonomyId,
                        SelectedTermId = part.TermId,
                        DisplayContentCount = part.DisplayContentCount,
                        DisplayTopMenuItem = part.DisplayRootTerm,
                        HideEmptyTerms = part.HideEmptyTerms,
                    };

                    if (updater != null) {
                        if (updater.TryUpdateModel(model, Prefix, null, null)) {
                            // taxonomy to render
                            part.TaxonomyId = model.SelectedTaxonomyId;
                            // root term (can be null)
                            part.TermId = model.SelectedTermId;
                            part.DisplayContentCount = model.DisplayContentCount;
                            part.DisplayRootTerm = model.DisplayTopMenuItem;
                            part.HideEmptyTerms = model.HideEmptyTerms;
                        }
                    }

                    var taxonomies = _taxonomyService.GetTaxonomies().ToArray();

                    var listItems = taxonomies.Select(taxonomy => new SelectListItem {
                        Value = Convert.ToString(taxonomy.Id),
                        Text = taxonomy.Name,
                        Selected = taxonomy.Id == part.TaxonomyId,
                    }).ToList();

                    model.AvailableTaxonomies = new SelectList(listItems, "Value", "Text", model.SelectedTaxonomyId);

                    // if no taxonomy is selected, take the first available one as 
                    // the terms drop down needs one by default
                    if (model.SelectedTaxonomyId <= 0) {
                        var firstTaxonomy = taxonomies.FirstOrDefault();
                        if (firstTaxonomy != null) {
                            model.SelectedTaxonomyId = firstTaxonomy.Id;
                        }
                    }

                    return shapeHelper.EditorTemplate(TemplateName: "Parts/Navigation.Taxonomy.Edit", Model: model, Prefix: Prefix);
                });
        }

    }
}