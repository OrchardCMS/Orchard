using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Contrib.Taxonomies.ViewModels;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyMenuPartDriver : ContentPartDriver<TaxonomyMenuPart> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentManager _contentManager;

        public TaxonomyMenuPartDriver(ITaxonomyService taxonomyService, IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
        }

        protected override string Prefix { get { return "TaxonomyMenu"; } }

        protected override DriverResult Display(TaxonomyMenuPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Taxonomies_Menu",
                () => {
                    var taxonomy = _taxonomyService.GetTaxonomy(part.TaxonomyPartRecord.Id);
                    var terms = new List<TermPart>();
                    if (taxonomy != null) {
                        var allTerms = _taxonomyService.GetTerms(taxonomy.Id).Select((t, i) => new { Level = t.GetLevels(), Term = t, Index = i }).ToList();

                        int index = 0, minLevel = 0, maxLevel = int.MaxValue - 1;
                        // display only a subset ?
                        if (part.TermPartRecord != null) {
                            index = allTerms.Where(t => t.Term.Id == part.TermPartRecord.Id).Single().Index;
                            minLevel = allTerms[index].Level;
                        }

                        if (part.LevelsToDisplay != 0) {
                            maxLevel = minLevel + part.LevelsToDisplay - 1;
                        }

                        // ignore top term ?
                        if (part.TermPartRecord != null && !part.DisplayTopMenuItem && allTerms.Any())
                        {
                            minLevel++;
                            maxLevel++;
                        }
                        else
                        {
                            // don't add it if it should be hidden
                            if (!part.HideEmptyTerms || allTerms[index].Term.Count > 0)
                            {
                                terms.Add(allTerms[index].Term);
                            }

                            if (part.DisplayTopMenuItem)
                            {
                                minLevel++;
                            }
                        }

                        for (index++; index < allTerms.Count; index++) {
                            var indexed = allTerms[index];
                            // ignore deep terms
                            if (indexed.Level > maxLevel) {
                                continue;
                            }

                            // stop looping when reached another branch
                            if (indexed.Level < minLevel) {
                                break;
                            }

                            if (part.HideEmptyTerms && indexed.Term.Count == 0) {
                                continue;
                            }

                            terms.Add(indexed.Term);
                        }
                    }

                    return shapeHelper.Parts_Taxonomies_Menu(ContentPart: part, Terms: terms, Taxonomy: taxonomy);
                });
        }

        protected override DriverResult Editor(TaxonomyMenuPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TaxonomyMenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Taxonomies_Menu_Edit", () => {
                    var model = new TaxonomyMenuViewModel {
                        SelectedTaxonomyId = part.TaxonomyPartRecord != null ? part.TaxonomyPartRecord.Id : -1,
                        SelectedTermId = part.TermPartRecord != null ? part.TermPartRecord.Id : -1,
                        DisplayContentCount = part.DisplayContentCount,
                        DisplayTopMenuItem = part.DisplayTopMenuItem,
                        HideEmptyTerms = part.HideEmptyTerms,
                        LevelsToDisplay = part.LevelsToDisplay
                    };

                    if (updater != null) {
                        if (updater.TryUpdateModel(model, Prefix, null, null)) {
                            var selectedTerm = _taxonomyService.GetTerm(model.SelectedTermId);

                            // taxonomy to render
                            part.TaxonomyPartRecord = _taxonomyService.GetTaxonomy(model.SelectedTaxonomyId).Record;
                            // root term (can be null)
                            part.TermPartRecord = selectedTerm == null ? null : selectedTerm.Record;
                            part.DisplayContentCount = model.DisplayContentCount;
                            part.DisplayTopMenuItem = model.DisplayTopMenuItem;
                            part.HideEmptyTerms = model.HideEmptyTerms;
                            part.LevelsToDisplay = model.LevelsToDisplay;
                        }
                    }

                    var taxonomies = _taxonomyService.GetTaxonomies();

                    var listItems = taxonomies.Select(taxonomy => new SelectListItem {
                        Value = Convert.ToString(taxonomy.Id),
                        Text = taxonomy.Name,
                        Selected = taxonomy.Record == part.TaxonomyPartRecord,
                    }).ToList();

                    model.AvailableTaxonomies = new SelectList(listItems, "Value", "Text", model.SelectedTaxonomyId);

                    // if no taxonomy is selected, take the first available one as 
                    // the terms drop down needs one by default
                    if(model.SelectedTaxonomyId == -1) {
                        var firstTaxonomy = taxonomies.FirstOrDefault();
                        if(firstTaxonomy != null) {
                            model.SelectedTaxonomyId = firstTaxonomy.Id;
                        }
                    }

                    return shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.Menu", Model: model, Prefix: Prefix);
                });
        }

        protected override void Exporting(TaxonomyMenuPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayTopMenuItem", part.DisplayTopMenuItem);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayContentCount", part.DisplayContentCount);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LevelsToDisplay", part.LevelsToDisplay);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HideEmptyTerms", part.HideEmptyTerms);

            if (part.TaxonomyPartRecord != null) {
                var taxonomy = _taxonomyService.GetTaxonomy(part.TaxonomyPartRecord.Id);
                context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyPartRecordId", _contentManager.GetItemMetadata(taxonomy).Identity);
            }
            if (part.TermPartRecord != null) {
                var term = _taxonomyService.GetTerm(part.TermPartRecord.Id);
                context.Element(part.PartDefinition.Name).SetAttributeValue("TermPartRecordId", _contentManager.GetItemMetadata(term).Identity);
            }
        }

        protected override void Importing(TaxonomyMenuPart part, ImportContentContext context) {
            var partName = part.PartDefinition.Name;

            part.DisplayTopMenuItem = GetAttribute<bool>(context, partName, "DisplayTopMenuItem");
            part.DisplayContentCount = GetAttribute<bool>(context, partName, "DisplayContentCount");
            part.LevelsToDisplay = GetAttribute<int>(context, partName, "LevelsToDisplay");
            part.HideEmptyTerms = GetAttribute<bool>(context, partName, "HideEmptyTerms");
        }

        protected override void Imported(TaxonomyMenuPart part, ImportContentContext context) {
            var taxonomyId = GetAttribute<string>(context, part.PartDefinition.Name, "TaxonomyPartRecordId");
            if (!string.IsNullOrWhiteSpace(taxonomyId)) {
                var taxonomy = context.GetItemFromSession(taxonomyId);
                if (taxonomy != null) {
                    part.TaxonomyPartRecord = taxonomy.As<TaxonomyPart>().Record;
                }
            }

            var termId = GetAttribute<string>(context, part.PartDefinition.Name, "TermPartRecordId");
            if (!string.IsNullOrWhiteSpace(termId)) {
                var term = context.GetItemFromSession(termId);
                if (term != null) {
                    part.TermPartRecord = term.As<TermPart>().Record;
                }
            }
        }

        public static T GetAttribute<T>(ImportContentContext context, string partName, string elementName) {
            string value = context.Attribute(partName, elementName);
            if (value != null) {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default(T);
        }
    }
}