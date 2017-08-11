using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;

namespace Orchard.Taxonomies.Handlers {
    public class TaxonomyPartHandler : ContentHandler {
        public TaxonomyPartHandler(
            IRepository<TaxonomyPartRecord> repository,
            ITaxonomyService taxonomyService,
            IContentDefinitionManager contentDefinitionManager,
            ILocalizationService localizationService = null) { //Localization feature may not be active

            string previousName = null;

            Filters.Add(StorageFilter.For(repository));
            OnPublished<TaxonomyPart>((context, part) => {

                if (part.TermTypeName == null) {
                    // is it a new taxonomy ?
                    taxonomyService.CreateTermContentType(part);
                }
                else {
                    // update existing fields
                    foreach (var partDefinition in contentDefinitionManager.ListPartDefinitions()) {
                        foreach (var field in partDefinition.Fields) {
                            if (field.FieldDefinition.Name == typeof(TaxonomyField).Name) {

                                if (field.Settings.GetModel<TaxonomyFieldSettings>().Taxonomy == previousName) {
                                    //could either be a name change, or we could be publishing a translation
                                    if (localizationService != null) { //Localization feature may not be active
                                        var locPart = part.ContentItem.As<LocalizationPart>();
                                        if (locPart != null) {
                                            var localizedTaxonomies = localizationService
                                                .GetLocalizations(part.ContentItem) //versions in all cultures
                                                .Where(pa => pa.ContentItem.Id != part.ContentItem.Id) //but not the one we are publishing
                                                .Select(pa => {
                                                    var tax = pa.ContentItem.As<TaxonomyPart>(); //the TaxonomyPart
                                                return tax == null ? string.Empty : tax.Name; //get its name (with sanity check)
                                            });
                                            if (localizedTaxonomies.Contains(previousName))
                                                continue; //this is a new localization, so move along
                                        }
                                    }
                                    contentDefinitionManager.AlterPartDefinition(partDefinition.Name,
                                        cfg => cfg.WithField(field.Name,
                                            builder => builder.WithSetting("TaxonomyFieldSettings.Taxonomy", part.Name)));
                                }
                            }
                        }
                    }
                }
            });

            OnLoading<TaxonomyPart>((context, part) => part.TermsField.Loader(() => taxonomyService.GetTerms(part.Id)));

            OnUpdating<TitlePart>((context, part) => {
                // if altering the title of a taxonomy, save the name
                if (part.As<TaxonomyPart>() != null) {
                    previousName = part.Title;
                }
            });
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var taxonomy = context.ContentItem.As<TaxonomyPart>();

            if (taxonomy == null)
                return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Taxonomies"},
                {"Controller", "Admin"},
                {"Action", "Edit"},
                {"Id", taxonomy.Id}
            };

        }
    }
}