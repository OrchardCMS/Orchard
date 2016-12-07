using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Helpers;
using Orchard.Localization.Services;
using Orchard.Localization.Models;
using System.Xml.Serialization;
using System.IO;
using System.Web.Script.Serialization;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly ICultureManager _cultureManager;
        public LocalizedTaxonomyFieldDriver(ILocalizationService localizationService, ITaxonomyService taxonomyService, ITaxonomyExtensionsService taxonomyExtensionsService, ICultureManager cultureManager) {
            _localizationService = localizationService;
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _cultureManager = cultureManager;
        }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }
        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            // return ContentShape("Fields_TaxonomyFieldList_Edit", GetDifferentiator(field, part), () => {
            return ContentShape("Fields_TaxonomyFieldList_Edit", GetDifferentiator(field, part), () => {
                // return BuildEditorShape(part, field, shapeHelper);
                string selectedCulture;
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                if (part.ContentItem.As<LocalizationPart>() != null) {
                    if (part.ContentItem.As<LocalizationPart>().Culture == null) {

                        selectedCulture = _cultureManager.GetSiteCulture();//for new contentItem with localization
                    }
                    else
                        selectedCulture = part.ContentItem.As<LocalizationPart>().Culture.Culture;
                }
                else {

                    var taxonomyPart = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);
                    if (taxonomyPart.ContentItem.As<LocalizationPart>() != null)
                        selectedCulture = taxonomyPart.ContentItem.As<LocalizationPart>().Culture.Culture;
                    else
                        selectedCulture = null; // CI without localization and taxonomy without localization
                }
                var localizationSetting = new LocalizationTaxonomyFieldSettings {
                    Taxonomy = settings.Taxonomy,
                    Hint = settings.Hint,
                    LeavesOnly = settings.LeavesOnly,
                    Required = settings.Required,
                    SingleChoice = settings.SingleChoice,
                    Autocomplete = settings.Autocomplete
                };

                // var serializedData = new JavaScriptSerializer().Serialize(localizationSetting);
                //   XmlSerializer xmlsetting = new XmlSerializer(localizationSetting.GetType());
                //string serializedData = "";
                //using (StringWriter sw = new StringWriter()) {
                //    xmlsetting.Serialize(sw, localizationSetting);
                //    serializedData = sw.ToString();
                //}
                var templateName = "Fields/TaxonomyFieldList"; //settings.Autocomplete ? "Fields/TaxonomyField.Autocomplete" : "Fields/TaxonomyField";
                var viewModel = new LocalizationTaxonomiesViewModel {
                    Cultures = _cultureManager.ListCultures(),
                    SelectedCulture = selectedCulture,
                    Settings = localizationSetting,
                    ContentType = part.ContentItem.ContentType,
                    ContentPart = part.PartDefinition.Name,
                    FieldName = field.Name,
                    Prefix = GetPrefix(field, part)
                    //,Prefix = "Localized_Taxonomy_" + contentItemTaxonomyMaster.Id
                };
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
            });
        }

        //protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
        //    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
        //    var localizationSettings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldLocalizationSettings>();
        //    if (part.ContentItem.As<LocalizationPart>() != null && part.ContentItem.As<LocalizationPart>().Culture != null) {
        //        var viewModel = new LocalizationTaxonomiesViewModel();
        //        if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
        //            if 
        //            if (localizationSettings.AssertSameCulture) {
        //                var taxonomyRelative = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);
        //                var contentItemTaxonomyMaster = _taxonomyExtensionsService.GetMasterItem(taxonomyRelative.ContentItem);
        //                var taxonomiesList = _localizationService.GetLocalizations(contentItemTaxonomyMaster, null).Concat(new[] { contentItemTaxonomyMaster.ContentItem.As<LocalizationPart>() });

        //                if (part.ContentItem.As<LocalizationPart>().Culture.Culture.Equals()) {
        //                }
        //            }
        //    }
        //    }
        //    return null;
        //}
        //     protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
        //    // Initializing viewmodel using the terms that are already selected to prevent loosing them when updating an editor group this field isn't displayed in.

        //    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
        //    var taxonomyRelative = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);
        //    var contentItemTaxonomyMaster = _taxonomyExtensionsService.GetMasterItem(taxonomyRelative.ContentItem);
        //    var taxonomiesList = _localizationService.GetLocalizations(contentItemTaxonomyMaster, null).Concat(new[] { contentItemTaxonomyMaster.ContentItem.As<LocalizationPart>() });




        //    //var terms = taxonomy != null && !settings.Autocomplete
        //    //          ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).ToList()
        //    //          : new List<TermEntry>(0);
        //    var viewModel = new LocalizedTaxonomyListViewModel();
        //    foreach (var taxonomy in taxonomiesList) {
        //        var subTerms = GetAppliedTerms(taxonomy, field, VersionOptions.Latest).ToList();
        //        var taxonomyFieldViewModel = new TaxonomyFieldViewModel { Terms = subTerms.Select(t => t.CreateTermEntry()).ToList() };
        //        foreach (var item in taxonomyFieldViewModel.Terms) item.IsChecked = true;
        //        viewModel.ListTaxonomy.Add(taxonomy.Culture.Culture, taxonomyFieldViewModel);
        //    }
        //    // da qui
        //    //var appliedTerms = GetAppliedTerms(part, field, VersionOptions.Latest).ToList();
        //    //var viewModel = new TaxonomyFieldViewModel { Terms = appliedTerms.Select(t => t.CreateTermEntry()).ToList() };
        //    //foreach (var item in viewModel.Terms) item.IsChecked = true;
        //   var Prefix = "Localized_Taxonomy_" + contentItemTaxonomyMaster.Id;
        //    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
        //    //    var checkedTerms = viewModel.Terms
        //    //        .Where(t => (t.IsChecked || t.Id == viewModel.SingleTermId))
        //    //        .Select(t => GetOrCreateTerm(t, viewModel.TaxonomyId, field))
        //    //        .Where(t => t != null).ToList();

        //    //    var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
        //    //    if (settings.Required && !checkedTerms.Any()) {
        //    //        updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
        //    //    }
        //    //    else
        //    //        _taxonomyService.UpdateTerms(part.ContentItem, checkedTerms, field.Name);
        //    }

        //    var templateName = "Fields/TaxonomyFieldList"; //settings.Autocomplete ? "Fields/TaxonomyField.Autocomplete" : "Fields/TaxonomyField";
        //    return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));

        //}



        private static string GetDifferentiator(TaxonomyField field, ContentPart part) {
            return field.Name;
        }
        private IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
            string fieldName = field != null ? field.Name : string.Empty;

            return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions ?? VersionOptions.Published).Distinct(new TermPartComparer());
        }
        //    private ContentShapeResult BuildEditorShape(ContentPart part, TaxonomyField field, dynamic shapeHelper, LocalizedTaxonomyListViewModel appliedViewModel = null) {
        //        return ContentShape("Fields_TaxonomyField_Edit", GetDifferentiator(field, part), () => {

        //            var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
        //            var appliedTerms = GetAppliedTerms(part, field, VersionOptions.Latest).ToDictionary(t => t.Id, t => t);
        //            var taxonomyRelative = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);

        //            //  if (((dynamic)taxobase.ContentItem).LocalizationPart.Culture.Culture != Language) {
        //            //      taxobase = _taxonomyService.GetTaxonomies().Where(x => (x.Id == idmaster || (((dynamic)x.ContentItem).LocalizationPart.MasterContentItem != null && ((dynamic)x.ContentItem).LocalizationPart.MasterContentItem.Id == idmaster)) && ((dynamic)x.ContentItem).LocalizationPart.Culture.Culture == Language).FirstOrDefault();
        //            //  }

        //            //  get all content with mastercontentid
        //            //  taxonomy.ContentItem.As<LocalizationPart>().MasterContentItem.Id;

        //            var contentItemTaxonomyMaster = _taxonomyExtensionsService.GetMasterItem(taxonomyRelative.ContentItem);
        //            var taxonomiesList = _localizationService.GetLocalizations(contentItemTaxonomyMaster, null).Concat(new[] { contentItemTaxonomyMaster.ContentItem.As<LocalizationPart>() });

        //            var viewModel = new LocalizedTaxonomyListViewModel() {
        //              //  ListTaxonomy = new Dictionary<string, TaxonomyFieldViewModel>(),
        //                Cultures = _cultureManager.ListCultures(),
        //                SelectedCulture = "it-IT",
        //                Prefix = "Localized_Taxonomy_" + contentItemTaxonomyMaster.Id
        //            };


        //            //    var taxonomiesList = _taxonomyService.GetTaxonomies().Where(x => (((dynamic)x.ContentItem).LocalizationPart.MasterContentItem.Id == taxonomyRelative.ContentItem.As<LocalizationPart>().MasterContentItem.Id));

        //            foreach (var taxonomy in taxonomiesList) {
        //                var terms = taxonomy != null && !settings.Autocomplete
        //                    ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).ToList()
        //                    : new List<TermEntry>(0);


        //                // Ensure the modified taxonomy items are not lost if a model validation error occurs
        //                if (appliedViewModel != null) {

        //                    // to do sostituire terms.ForEach(t => t.IsChecked = appliedViewModel.Terms.Any(at => at.Id == t.Id && at.IsChecked) || t.Id == appliedViewModel.SingleTermId);
        //                }
        //                else {
        //                    terms.ForEach(t => t.IsChecked = appliedTerms.ContainsKey(t.Id));
        //                }
        //                viewModel.ListTaxonomy.Add(taxonomy.Culture.Culture, new TaxonomyFieldViewModel {
        //                    DisplayName = field.DisplayName,
        //                    Name = field.Name,
        //                    Terms = terms,
        //                    SelectedTerms = appliedTerms.Select(t => t.Value),
        //                    Settings = settings,
        //                    SingleTermId = appliedTerms.Select(t => t.Key).FirstOrDefault(),
        //                    TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
        //                    HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
        //                });
        //            }
        //            var templateName = "Fields/TaxonomyFieldList"; //settings.Autocomplete ? "Fields/TaxonomyField.Autocomplete" : "Fields/TaxonomyField";
        //            return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
        //        });

        //    }
        //
    }
}