using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Drivers {
    public class TaxonomyNavigationPartDriver : ContentPartDriver<TaxonomyNavigationPart> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;

        public TaxonomyNavigationPartDriver(ITaxonomyService taxonomyService, IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

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
                        LevelsToDisplay = part.LevelsToDisplay,
                    };

                    if (updater != null) {
                        if (updater.TryUpdateModel(model, Prefix, null, null)) {

                            if (model.LevelsToDisplay < 0) {
                                updater.AddModelError("LevelsToDisplay", T("The levels to display must be a positive number"));
                            }
                            else {
                                // taxonomy to render
                                part.TaxonomyId = model.SelectedTaxonomyId;
                                // root term (can be null)
                                part.TermId = model.SelectedTermId;
                                part.DisplayContentCount = model.DisplayContentCount;
                                part.DisplayRootTerm = model.DisplayTopMenuItem;
                                part.HideEmptyTerms = model.HideEmptyTerms;
                                part.LevelsToDisplay = model.LevelsToDisplay;
                            }
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

        protected override void Exporting(TaxonomyNavigationPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayContentCount", part.DisplayContentCount);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayRootTerm", part.DisplayRootTerm);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HideEmptyTerms", part.HideEmptyTerms);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LevelsToDisplay", part.LevelsToDisplay);

            var taxonomy = _contentManager.Get(part.TaxonomyId);
            var taxonomyId = _contentManager.GetItemMetadata(taxonomy).Identity.ToString();

            context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyId", taxonomyId);

            if (part.TermId != -1) {
                var term = _contentManager.Get(part.TermId);
                var termId = _contentManager.GetItemMetadata(term).Identity.ToString();

                context.Element(part.PartDefinition.Name).SetAttributeValue("TermId", termId);
            }
        }

        protected override void Importing(TaxonomyNavigationPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.DisplayContentCount = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "DisplayContentCount"));
            part.DisplayRootTerm = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "DisplayRootTerm"));
            part.HideEmptyTerms = Boolean.Parse(context.Attribute(part.PartDefinition.Name, "HideEmptyTerms"));
            part.LevelsToDisplay = Int32.Parse(context.Attribute(part.PartDefinition.Name, "LevelsToDisplay"));

            var taxonomyId = context.Attribute(part.PartDefinition.Name, "TaxonomyId");
            var taxonomy = context.GetItemFromSession(taxonomyId);

            if (taxonomy == null) {
                throw new OrchardException(T("Unknown taxonomy: {0}", taxonomyId));
            }

            part.TaxonomyId = taxonomy.Id;

            var termId = context.Attribute(part.PartDefinition.Name, "TermId");
            if (!String.IsNullOrEmpty(termId)) {
                var term = context.GetItemFromSession(termId);

                if (term == null) {
                    throw new OrchardException(T("Unknown term: {0}", termId));
                }

                part.TermId = term.Id;
            }
        }
    }
}