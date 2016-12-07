using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Helpers;
using System.Web.Script.Serialization;

namespace Orchard.Taxonomies.Controllers {
    public class LocalizationTaxonomyController : Controller {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomySource _taxonomySource;
        private readonly ICultureManager _cultureManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IContentManager _contentManager;
        public IOrchardServices Services { get; set; }
        public IFeatureManager _featureManager { get; set; }
        public LocalizationTaxonomyController(
                IOrchardServices services,
                ITaxonomyService taxonomyService,
                ITaxonomySource taxonomySource,
                //              IRepository<TermContentItem> repository,
                IFeatureManager featureManager,
                ICultureManager cultureManager,
                IShapeFactory shapeFactory,
                IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _taxonomySource = taxonomySource;
            _shapeFactory = shapeFactory;
            Services = services;
            _featureManager = featureManager;
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        private IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
            string fieldName = field != null ? field.Name : string.Empty;
            return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions ?? VersionOptions.Published).Distinct(new TermPartComparer());
        }
        public ActionResult GetTaxonomy(string contentTypeName, string contentPartName, string fieldName, string culture,    
            // string contentTypeName, string contentPartName, string fieldName, string culture) {
        string Taxonomy, string Hint, bool LeavesOnly, bool Required, bool SingleChoice, bool Autocomplete) {


            LocalizationTaxonomyFieldSettings taxonomySettings = new LocalizationTaxonomyFieldSettings {
                Taxonomy = Taxonomy,
                Hint = Hint,
                LeavesOnly = LeavesOnly,
                Required = Required,
                SingleChoice = Autocomplete
            };
            //var serializer = new JavaScriptSerializer();
            //var taxonomySettings = (LocalizationTaxonomyFieldSettings)serializer.DeserializeObject(setting);

            ContentItem FakeContent = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord()
                },
                ContentType = contentTypeName
            };
            FakeContent.Record.Id = -1;
            var localizationPart = new LocalizationPart();
            var partwithField = new ContentPart() { TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition(contentPartName), new SettingsDictionary()) };
            FakeContent.Weld(partwithField);
            var fakeField = new TaxonomyField { PartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition("TaxonomyField"), fieldName, new SettingsDictionary())};
            FakeContent.Weld(localizationPart);
            partwithField.Weld(fakeField);
            FakeContent.As<LocalizationPart>().Culture = _cultureManager.GetCultureByName(culture);

            

          //  LocalizationTaxonomyFieldSettings taxonomySettings = new LocalizationTaxonomyFieldSettings();
           // taxonomySettings.Taxonomy = fieldName;
            //  TaxonomyFieldSettings taxonomySettings = new Settings.TaxonomyFieldSettings();

            //XmlSerializer deserializer = new XmlSerializer(typeof(LocalizationTaxonomyFieldSettings));
            //using (TextReader tr = new StringReader(localizationTaxonomyFieldSettings)) {
            //    taxonomySettings = (LocalizationTaxonomyFieldSettings)deserializer.Deserialize(tr);
            //}




            //   var contentItemTaxonomy=_contentManager.Get(taxonomtId);


            var appliedTerms = GetAppliedTerms(partwithField, fakeField, VersionOptions.Latest).ToDictionary(t => t.Id, t => t);
            var taxonomy = _taxonomySource.GetTaxonomy(taxonomySettings.Taxonomy, FakeContent);

            var terms = taxonomy != null && !taxonomySettings.Autocomplete
                ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).ToList()
                : new List<TermEntry>(0);

            // Ensure the modified taxonomy items are not lost if a model validation error occurs
            //if (appliedViewModel != null) {
            //    terms.ForEach(t => t.IsChecked = appliedViewModel.Terms.Any(at => at.Id == t.Id && at.IsChecked) || t.Id == appliedViewModel.SingleTermId);
            //}
            //else {
                terms.ForEach(t => t.IsChecked = appliedTerms.ContainsKey(t.Id));
            //          }
            TaxonomyFieldSettings tfs = new TaxonomyFieldSettings {
                Taxonomy = taxonomySettings.Taxonomy,
                Hint = taxonomySettings.Hint,
                LeavesOnly = taxonomySettings.LeavesOnly,
                Required = taxonomySettings.Required,
                SingleChoice = taxonomySettings.SingleChoice,
                Autocomplete = taxonomySettings.Autocomplete
            };
      
                      var viewModel = new TaxonomyFieldViewModel {
                DisplayName = taxonomySettings.Taxonomy,
                Name = fakeField.Name,
                Terms = terms,
                SelectedTerms = appliedTerms.Select(t => t.Value),
                Settings = tfs,
                SingleTermId = appliedTerms.Select(t => t.Key).FirstOrDefault(),
                TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
            };


            return View("TaxonomyField", viewModel);







            ////  var shape = shapeHelper();
            ////  Assert.That(part.Has(typeof(TaxonomyField), fieldName));
            //var driver = new TaxonomyFieldDriver(Services, _taxonomyService, _taxonomySource, _featureManager) as IContentFieldDriver;
            //((TaxonomyFieldDriver)driver).BuildEditorShape(partwithField, (TaxonomyField)fakeField, _shapeFactory);
 
        }

            //public ActionResult GetTaxonomy(string contentTypeName, string contentPartName, string fieldName, string culture) {


            //    ContentItem FakeContent = new ContentItem {
            //        VersionRecord = new ContentItemVersionRecord {
            //            ContentItemRecord = new ContentItemRecord()
            //        },
            //        ContentType = contentTypeName
            //    };
            //    FakeContent.Record.Id = -1;
            //    var localizationPart = new LocalizationPart();


            //    var partwithField = new ContentPart() { TypePartDefinition = new ContentTypePartDefinition(new ContentPartDefinition(contentPartName), new SettingsDictionary()) };
            //    FakeContent.Weld(partwithField);
            //    var fakeField = new TaxonomyField { PartFieldDefinition = new ContentPartFieldDefinition(new ContentFieldDefinition("TaxonomyField"), fieldName, new SettingsDictionary()) };
            //    FakeContent.Weld(localizationPart);
            //    partwithField.Weld(fakeField);
            //    FakeContent.As<LocalizationPart>().Culture = _cultureManager.GetCultureByName(culture);
            //    //  var shape = shapeHelper();
            //    //  Assert.That(part.Has(typeof(TaxonomyField), fieldName));
            //    var driver = new TaxonomyFieldDriver(Services, _taxonomyService, _taxonomySource, _featureManager) as IContentFieldDriver;
            //    //  var field = new TaxonomyField();

            //  //  _shapeFactory.Create(actualShapeType, Arguments.Empty(), () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty())))
            //    //dynamic shapehelper = null;
            //    //IShapeFactory shapeFactory = context.New;
            //    //return shapeFactory.Create(shapeType);
            //    var contentShapeProvider = ((TaxonomyFieldDriver)driver).BuildEditorShape(partwithField, (TaxonomyField)fakeField, _shapeFactory);
            //    return contentShapeProvider;

            //    return View(contentShapeProvider);
            //    //      return "aa";
            //    //ContentHelpers.PreparePart<TaxonomyField>(part, "UspsShippingMethod");
            //    //var context = new ImportContentContext(
            //    //    part.ContentItem, doc, new ImportContentSession(null));
            //    //driver.Importing(context);

            //    //Assert.That(part.Name, Is.EqualTo("Foo"));
            //    //Assert.That(part.Size, Is.EqualTo("L"));
            //    //Assert.That(part.WidthInInches, Is.EqualTo(10));
            //    //Assert.That(part.LengthInInches, Is.EqualTo(11));
            //    //Assert.That(part.HeightInInches, Is.EqualTo(12));
            //    //Assert.That(part.MaximumWeightInOunces, Is.EqualTo(1.3));
            //    //Assert.That(part.Priority, Is.EqualTo(14));
            //    //Assert.That(part.International, Is.True);
            //    //Assert.That(part.RegisteredMail, Is.True);
            //    //Assert.That(part.Insurance, Is.False);
            //    //Assert.That(part.ReturnReceipt, Is.True);
            //    //Assert.That(part.CertificateOfMailing, Is.True);
            //    //Assert.That(part.ElectronicConfirmation, Is.True);
            //    //return "ssss";
            //}




        }



    //    public class ContentHelpers {
    //        public static ContentItem PrepareField<TPart>(TPart part, string contentType, int id = -1)
    //where TPart : ContentPart<TRecord>
    //where TRecord : ContentPartRecord, new() {

    //            part.Record = new TRecord();
    //            var contentItem = part.ContentItem = new ContentItem {
    //                VersionRecord = new ContentItemVersionRecord {
    //                    ContentItemRecord = new ContentItemRecord()
    //                },
    //                ContentType = contentType
    //            };
    //            contentItem.Record.Id = id;
    //            contentItem.Weld(part);
    //            return contentItem;
    //        }
    //   }
}