using Orchard.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Drivers;
using Orchard.Taxonomies.Models;
using Orchard.Localization;

namespace Orchard.Taxonomies.Drivers {
    public class TaxonomyPartDriver : ContentPartDriver<TaxonomyPart> {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyPartDriver(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        protected override string Prefix { get { return "Taxonomy"; } }
        public Localizer T { get; set; }

        protected override DriverResult Display(TaxonomyPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_TaxonomyPart", () => {
                var taxonomyShape = shapeHelper.Taxonomy(ContentPart: part, ContentItem: part.ContentItem);
                var terms = _taxonomyService.GetTerms(part.ContentItem.Id);
                _taxonomyService.CreateHierarchy(terms, (parent, child) => {

                    if (child.Shape == null) {
                        child.Shape = shapeHelper.TaxonomyItem(Taxonomy: part.ContentItem, ContentPart: child.TermPart, ContentItem: child.TermPart.ContentItem);
                    }

                    // adding to root
                    if (parent.TermPart == null) {
                        taxonomyShape.Items.Add(child.Shape);
                    }
                    else {
                                
                        if (parent.Shape == null) {
                            parent.Shape = shapeHelper.TaxonomyItem(Taxonomy: part.ContentItem, ContentPart: parent.TermPart, ContentItem: parent.TermPart.ContentItem);
                        }

                        parent.Shape.Items.Add(child.Shape);
                    }

                });

                return shapeHelper.Parts_TaxonomyPart(Taxonomy: taxonomyShape);
            });
        }

        protected override DriverResult Editor(TaxonomyPart part, IUpdateModel updater, dynamic shapeHelper) {
            var existing = _taxonomyService.GetTaxonomyByName(part.Name);

            if (existing != null && existing.Record != part.Record) {
                updater.AddModelError("Title", T("A taxonomy with the same name already exists"));
            }
            
            // nothing to display for this part
            return null;
        }

        protected override void Exporting(TaxonomyPart part, ExportContentContext context) {
            if (part.IsInternal) {
                context.Exclude = true;
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("TermTypeName", part.TermTypeName);
        }

        protected override void Importing(TaxonomyPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.TermTypeName = context.Attribute(part.PartDefinition.Name, "TermTypeName");
        }
    }
}