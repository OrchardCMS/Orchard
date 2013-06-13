using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Localization;

namespace Orchard.Taxonomies.Drivers {
    public class TermWidgetPartDriver : ContentPartDriver<TermWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TermWidgetPartDriver(
            IContentManager contentManager, 
            ITaxonomyService taxonomyService,
            IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(TermWidgetPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_TermWidget_List",
                () => {
                    var termPart = _taxonomyService.GetTerm(part.TermPartRecord.Id);
                    var query = _taxonomyService.GetContentItemsQuery(termPart, part.FieldName);

                    Expression<Func<CommonPartRecord, DateTime?>> orderBy = d => d.CreatedUtc;
                    switch(part.OrderBy) {
                        case "Created": orderBy = d => d.CreatedUtc; break;
                        case "Published": orderBy = d => d.PublishedUtc; break;
                        case "Modified": orderBy = d => d.ModifiedUtc; break;
                    }

                    var results = query.Join<CommonPartRecord>().OrderByDescending(orderBy);

                    if(!String.IsNullOrWhiteSpace(part.ContentType)) {
                        results = results.ForType(part.ContentType).Join<CommonPartRecord>();
                    }

                    // build the Summary display for each content item
                    var list = shapeHelper.List();
                    list.AddRange(
                        results
                            .Slice(0, part.Count)
                            .ToList()
                            .Select( tp => _contentManager.BuildDisplay(tp.ContentItem, "Summary"))
                    );

                    return shapeHelper.Parts_TermWidget_List(ContentPart: part, ContentItems: list);
                });
        }

        protected override DriverResult Editor(TermWidgetPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(TermWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new TermWidgetViewModel {
                Part = part,
                ContentTypeNames = GetContentTypes(),
                Count = part.Count,
                FieldName = part.FieldName,
                OrderBy = part.OrderBy,
                SelectedTaxonomyId = part.TaxonomyPartRecord != null ? part.TaxonomyPartRecord.Id : -1,
                SelectedTermId = part.TermPartRecord != null ? part.TermPartRecord.Id : -1
            };

            if(updater != null) {
                if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                    var selectedTerm = _taxonomyService.GetTerm(viewModel.SelectedTermId);

                    // taxonomy to render
                    part.TaxonomyPartRecord = _taxonomyService.GetTaxonomy(viewModel.SelectedTaxonomyId).Record;
                    // root term (can be null)
                    part.TermPartRecord = selectedTerm == null ? null : selectedTerm.Record;
                    part.FieldName = viewModel.FieldName;
                    part.Count = viewModel.Count;
                    part.OrderBy = viewModel.OrderBy;
                }
            }

            var taxonomies = _taxonomyService.GetTaxonomies().ToList();

            var listItems = taxonomies.Select(taxonomy => new SelectListItem {
                Value = Convert.ToString(taxonomy.Id),
                Text = taxonomy.Name,
                Selected = taxonomy.Record == part.TaxonomyPartRecord,
            }).ToList();

            viewModel.AvailableTaxonomies = new SelectList(listItems, "Value", "Text", viewModel.SelectedTaxonomyId);

            // if no taxonomy is selected, take the first available one as 
            // the terms drop down needs one by default
            if (viewModel.SelectedTaxonomyId == -1) {
                var firstTaxonomy = taxonomies.FirstOrDefault();
                if (firstTaxonomy != null) {
                    viewModel.SelectedTaxonomyId = firstTaxonomy.Id;
                }
            }

            return ContentShape("Parts_Taxonomy_TermWidget_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Taxonomies.TermWidget", Model: viewModel, Prefix: Prefix));
        }

        private IEnumerable<string> GetContentTypes() {
            return _contentDefinitionManager
                .ListTypeDefinitions()
                .Select(t => t.Name)
                .OrderBy(x => x);
        }

        protected override void Importing(TermWidgetPart part, ImportContentContext context) {
            // importing taxonomy
            var taxonomyIdentity = context.Attribute(part.PartDefinition.Name, "TaxonomyId");
            var taxonomy = context.GetItemFromSession(taxonomyIdentity);

            if (taxonomy == null) {
                throw new OrchardException(T("Unknown taxonomy: {0}", taxonomyIdentity));
            }

            part.TaxonomyPartRecord = taxonomy.As<TaxonomyPart>().Record;

            //importing term
            var termIdentity = context.Attribute(part.PartDefinition.Name, "TermId");
            var term = context.GetItemFromSession(termIdentity);

            part.TermPartRecord = term.As<TermPart>().Record;

            if (term == null) {
                throw new OrchardException(T("Unknown term: {0}", termIdentity));
            } 

            // importing properties
            part.FieldName = context.Attribute(part.PartDefinition.Name, "FieldName");
            part.Count = Int32.Parse(context.Attribute(part.PartDefinition.Name, "Count"));
            part.OrderBy = context.Attribute(part.PartDefinition.Name, "OrderBy");
        }

        protected override void Exporting(TermWidgetPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("FieldName", part.FieldName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OrderBy", part.OrderBy);

            var taxonomy = _contentManager.Get(part.TaxonomyPartRecord.Id);
            var taxonomyIdentity = _contentManager.GetItemMetadata(taxonomy).Identity.ToString();
            context.Element(part.PartDefinition.Name).SetAttributeValue("TaxonomyId", taxonomyIdentity);

            var term = _contentManager.Get(part.TermPartRecord.Id);
            var termIdentity = _contentManager.GetItemMetadata(term).Identity.ToString();
            context.Element(part.PartDefinition.Name).SetAttributeValue("TermId", termIdentity);
        }
    }
}