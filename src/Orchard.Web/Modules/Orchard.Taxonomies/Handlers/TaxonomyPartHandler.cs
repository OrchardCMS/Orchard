using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Taxonomies.Settings;
using System;

namespace Orchard.Taxonomies.Handlers {
    [UsedImplicitly]
    public class TaxonomyPartHandler : ContentHandler {
        public TaxonomyPartHandler(
            IRepository<TaxonomyPartRecord> repository, 
            ITaxonomyService taxonomyService,
            IContentDefinitionManager contentDefinitionManager) {
            
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
                            if (field.FieldDefinition.Name == typeof (TaxonomyField).Name) {

                                if (field.Settings.GetModel<TaxonomyFieldSettings>().Taxonomy == previousName) {
                                    contentDefinitionManager.AlterPartDefinition(partDefinition.Name, 
                                        cfg => cfg.WithField(field.Name, 
                                            builder => builder.WithSetting("TaxonomyFieldSettings.Taxonomy", part.Name)));
                                }
                            }
                        }
                    }
                }
            });

            OnLoading<TaxonomyPart>( (context, part) => part.TermsField.Loader(x => taxonomyService.GetTerms(part.Id)));

            OnUpdating<TitlePart>((context, part) => {
                // if altering the title of a taxonomy, save the name
                if (part.As<TaxonomyPart>() != null) {
                    previousName = part.Title;
                }
            });
        }
    }
}