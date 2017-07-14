using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Helpers;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Drivers {
    public class TaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomySource _taxonomySource;
        public IOrchardServices Services { get; set; }

        public TaxonomyFieldDriver(
            IOrchardServices services, 
            ITaxonomyService taxonomyService,
            IRepository<TermContentItem> repository,
             ITaxonomySource taxonomySource) {
            _taxonomyService = taxonomyService;
            _taxonomySource = taxonomySource;
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

            return ContentShape("Fields_TaxonomyField", GetDifferentiator(field, part),
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                    var terms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name).ToList();
                    var taxonomy = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);

                    return shapeHelper.Fields_TaxonomyField(
                        ContentField: field,
                        Terms: terms,
                        Settings: settings,
                        Taxonomy: taxonomy);
                });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            return BuildEditorShape(part, field, shapeHelper);

        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
            // Initializing viewmodel using the terms that are already selected to prevent loosing them when updating an editor group this field isn't displayed in.
            var appliedTerms = GetAppliedTerms(part, field, VersionOptions.Latest).ToList();
            var viewModel = new TaxonomyFieldViewModel { Terms = appliedTerms.Select(t => t.CreateTermEntry()).Where(te => !te.HasDraft).ToList() };
            foreach (var item in viewModel.Terms) item.IsChecked = true;
            
            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                var checkedTerms = viewModel.Terms
                    .Where(t => (t.IsChecked || t.Id == viewModel.SingleTermId))
                    .Select(t => GetOrCreateTerm(t, viewModel.TaxonomyId, field))
                    .Where(t => t != null).ToList();

                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                if (settings.Required && !checkedTerms.Any()) {
                    updater.AddModelError(GetPrefix(field, part), T("The {0} field is required.", T(field.DisplayName)));
                }
                else
                    _taxonomyService.UpdateTerms(part.ContentItem, checkedTerms, field.Name);
            }

            return BuildEditorShape(part, field, shapeHelper, viewModel);
        }

        private ContentShapeResult BuildEditorShape(ContentPart part, TaxonomyField field, dynamic shapeHelper, TaxonomyFieldViewModel appliedViewModel = null) {
            return ContentShape("Fields_TaxonomyField_Edit", GetDifferentiator(field, part), () => {
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                var appliedTerms = GetAppliedTerms(part, field, VersionOptions.Latest).ToDictionary(t => t.Id, t => t);
                var taxonomy = _taxonomySource.GetTaxonomy(settings.Taxonomy, part.ContentItem);
                var terms = taxonomy != null && !settings.Autocomplete
                    ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).Where(te => !te.HasDraft).ToList()
                    : new List<TermEntry>(0);

                // Ensure the modified taxonomy items are not lost if a model validation error occurs
                if (appliedViewModel != null) {
                    terms.ForEach(t => t.IsChecked = appliedViewModel.Terms.Any(at => at.Id == t.Id && at.IsChecked) || t.Id == appliedViewModel.SingleTermId);
                }
                else {
                    terms.ForEach(t => t.IsChecked = appliedTerms.ContainsKey(t.Id));
                }

                var viewModel = new TaxonomyFieldViewModel {
                    DisplayName = field.DisplayName,
                    Name = field.Name,
                    Terms = terms,
                    SelectedTerms = appliedTerms.Select(t => t.Value),
                    Settings = settings,
                    SingleTermId = appliedTerms.Select(t => t.Key).FirstOrDefault(),
                    TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                    HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
                };

                var templateName = settings.Autocomplete ? "Fields/TaxonomyField.Autocomplete" : "Fields/TaxonomyField";
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
            });
        }

        protected override void Exporting(ContentPart part, TaxonomyField field, ExportContentContext context) {
            var appliedTerms = _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, field.Name);    
            // stores all content items associated to this field
            var termIdentities = appliedTerms.Select(x => Services.ContentManager.GetItemMetadata(x).Identity.ToString()).ToArray();
            context.Element(XmlConvert.EncodeLocalName(field.FieldDefinition.Name + "." + field.Name)).SetAttributeValue("Terms", String.Join(",", termIdentities));
        }

        protected override void Importing(ContentPart part, TaxonomyField field, ImportContentContext context) {
            var termIdentities = context.Attribute(XmlConvert.EncodeLocalName(field.FieldDefinition.Name + "." + field.Name), "Terms");
            if (termIdentities == null) {
                return;
            }

            var terms = termIdentities
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(context.GetItemFromSession)
                            .Where(contentItem => contentItem != null)
                            .ToList();

            _taxonomyService.UpdateTerms(part.ContentItem, terms.Select(x => x.As<TermPart>()), field.Name);
        }

        protected override void Cloning(ContentPart part, TaxonomyField originalField, TaxonomyField cloneField, CloneContentContext context) {
            _taxonomyService.UpdateTerms(context.CloneContentItem, originalField.Terms, cloneField.Name);
        }

        private TermPart GetOrCreateTerm(TermEntry entry, int taxonomyId, TaxonomyField field) {
            var term = default(TermPart);

            if (entry.Id > 0)            
                term = _taxonomyService.GetTerm(entry.Id);            
                 
            //Prevents creation of existing term
            if (term == null && !string.IsNullOrEmpty(entry.Name))
                term = _taxonomyService.GetTermByName(taxonomyId, entry.Name.Trim());

            if (term == null) {
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();

                if (!settings.AllowCustomTerms || !Services.Authorizer.Authorize(Permissions.CreateTerm)) {
                    Services.Notifier.Error(T("You're not allowed to create new terms for this taxonomy"));
                    return null;
                }

                var taxonomy = _taxonomyService.GetTaxonomy(taxonomyId);
                term = _taxonomyService.NewTerm(taxonomy);
                term.Name = entry.Name.Trim();
                term.Selectable = true;

                Services.ContentManager.Create(term, VersionOptions.Published);
                Services.Notifier.Success(T("The {0} term has been created.", term.Name));
            }

            return term;
        }


        private IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
            string fieldName = field != null ? field.Name : string.Empty;

            return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions?? VersionOptions.Published).Distinct(new TermPartComparer());
        }
    }

    internal class TermPartComparer : IEqualityComparer<TermPart> {
        public bool Equals(TermPart x, TermPart y) {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(TermPart obj) {
            return obj.Id.GetHashCode();
        }
    }
}
