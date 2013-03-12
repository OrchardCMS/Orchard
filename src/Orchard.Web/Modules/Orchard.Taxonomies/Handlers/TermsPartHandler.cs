using System.Linq;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace Orchard.Taxonomies.Handlers {
    public class TermsPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public TermsPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<TermsPartRecord> repository,
            ITaxonomyService taxonomyService,
            IContentManager contentManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            OnPublished<TermsPart>((context, part) => RecalculateCount(taxonomyService, part));
            OnUnpublished<TermsPart>((context, part) => RecalculateCount(taxonomyService, part));
            OnRemoved<TermsPart>((context, part) => RecalculateCount(taxonomyService, part));

            // Tells how to load the field terms on demand
            OnLoaded<TermsPart>((context, part) => {
                foreach (var field in part.ContentItem.Parts.SelectMany(p => p.Fields).OfType<TaxonomyField>()) {
                    var tempField = field.Name;
                    var fieldTermRecordIds = part.Record.Terms.Where(t => t.Field == tempField).Select(tci => tci.TermRecord.Id);
                    field.TermsField.Loader(value => fieldTermRecordIds.Select(id => _contentManager.Get<TermPart>(id)).ToList());
                }

                part._termParts.Loader(value => 
                    part.Terms.Select(
                        x => new TermContentItemPart { Field = x.Field, TermPart = _contentManager.Get<TermPart>(x.TermRecord.Id) }
                    ));
            });

            OnIndexing<TermsPart>(
                (context, part) => {

                    foreach (var term in part.Terms) {
                        var termContentItem = context.ContentManager.Get(term.TermRecord.Id);
                        context.DocumentIndex.Add(term.Field, termContentItem.As<TitlePart>().Title).Analyze();
                        context.DocumentIndex.Add(term.Field + "-id", termContentItem.Id).Store();
                        // tag the current content item with all parent terms
                        foreach (var parent in taxonomyService.GetParents(termContentItem.As<TermPart>())) {
                            context.DocumentIndex.Add(term.Field + "-id", parent.Id).Store();
                        }
                    }
                });
        }

        private static void RecalculateCount(ITaxonomyService taxonomyService, TermsPart part) {
            foreach (var term in part.Terms) {
                var termPart = taxonomyService.GetTerm(term.TermRecord.Id);
                term.TermRecord.Count = (int)taxonomyService.GetContentItemsCount(termPart);
            }
        }

        protected override void Activating(ActivatingContentContext context) {
            base.Activating(context);

            // weld the TermsPart dynamically, if a field has been assigned to one of its parts
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentType);
            if (contentTypeDefinition == null) {
                return;
            }

            if (contentTypeDefinition.Parts.Any(
                part => part.PartDefinition.Fields.Any(
                    field => field.FieldDefinition.Name == typeof(TaxonomyField).Name))) {

                context.Builder.Weld<TermsPart>();
            }
        }
    }
}