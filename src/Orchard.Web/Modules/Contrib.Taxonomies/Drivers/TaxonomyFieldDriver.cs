using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Models;
using JetBrains.Annotations;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Contrib.Taxonomies.Fields;
using Contrib.Taxonomies.Services;
using Contrib.Taxonomies.Settings;
using Contrib.Taxonomies.ViewModels;
using Contrib.Taxonomies.Helpers;

namespace Contrib.Taxonomies.Drivers {
    [UsedImplicitly]
    public class TaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private readonly ITaxonomyService _taxonomyService;
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/Contrib.TaxonomyField";

        public TaxonomyFieldDriver(
            IOrchardServices services, 
            ITaxonomyService taxonomyService,
            IRepository<TermContentItem> repository) {
            _taxonomyService = taxonomyService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(TaxonomyField field, ContentPart part) {
            return field.Name;
        }
        protected override DriverResult Display(ContentPart part, TaxonomyField field, string displayType, dynamic shapeHelper) {

            return ContentShape("Fields_Contrib_TaxonomyField", GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                    var terms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).ToList();
                    var taxonomy = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);

                    return shapeHelper.Fields_Contrib_TaxonomyField(
                        ContentField: field,
                        Terms: terms,
                        Settings: settings,
                        Taxonomy: taxonomy);
                });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();

            var appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).ToDictionary(t => t.Id, t => t);
            var taxonomy = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);
            var terms = _taxonomyService.GetTerms(taxonomy.Id).Select(t => t.CreateTermEntry()).ToList();

            terms.ForEach(t => t.IsChecked = appliedTerms.ContainsKey(t.Id));

            var viewModel = new TaxonomyFieldViewModel {
                Name = field.Name,
                Terms = terms,
                Settings = settings,
                SingleTermId = terms.Where(t => t.IsChecked).Select(t => t.Id).FirstOrDefault()
            };

            return ContentShape("Fields_Contrib_TaxonomyField_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new TaxonomyFieldViewModel { Terms =  new List<TermEntry>() };

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                var checkedTerms = viewModel.Terms.Where(t => t.IsChecked || t.Id == viewModel.SingleTermId).Select(t => _taxonomyService.GetTerm(t.Id)).ToList();
                _taxonomyService.UpdateTerms(part.ContentItem, checkedTerms, field.Name);
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Exporting(ContentPart part, TaxonomyField field, ExportContentContext context) {
            var appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name);

            // stores all content items associated to this field
            var termIdentities = appliedTerms.Select(x => Services.ContentManager.GetItemMetadata(x).Identity.ToString())
                .ToArray();

            context.Element(field.FieldDefinition.Name + "." + field.Name).SetAttributeValue("Terms", String.Join(",", termIdentities));
        }

        protected override void Importing(ContentPart part, TaxonomyField field, ImportContentContext context) {
            var termIdentities = context.Attribute(field.FieldDefinition.Name + "." + field.Name, "Terms");
            if (termIdentities == null) {
                return;
            }

            var terms = new List<ContentItem>();

            foreach (var identity in termIdentities.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                var contentItem = context.GetItemFromSession(identity);

                if (contentItem == null) {
                    continue;
                }

                terms.Add(contentItem);
            }

            _taxonomyService.UpdateTerms(part.ContentItem, terms.Select(x => x.As<TermPart>()), field.Name);
        }
    }
}
