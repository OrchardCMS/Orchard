using System.Linq;
using Contrib.Taxonomies.Fields;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
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

            OnPublished<TermsPart>((context, part) => RecalculateCount(contentManager, taxonomyService, part));
            OnUnpublished<TermsPart>((context, part) => RecalculateCount(contentManager, taxonomyService, part));
            OnRemoved<TermsPart>((context, part) => RecalculateCount(contentManager, taxonomyService, part));
            
            // tells how to load the field terms on demand
            OnLoaded<TermsPart>((context, part) => {
                foreach(var field in part.ContentItem.Parts.SelectMany(p => p.Fields).OfType<TaxonomyField>()) {
                    var tempField = field.Name;
                    var fieldTermRecordIds = part.Record.Terms.Where(t => t.Field == tempField).Select(tci => tci.TermRecord.Id);
                    field.Terms.Loader(value => fieldTermRecordIds.Select(id => _contentManager.Get<TermPart>(id)));
                }
            });

            OnIndexing<TermsPart>(
                (context, part) => {
                    foreach (var term in part.Terms) {
                        var value = context.ContentManager.Get(term.TermRecord.Id).As<TitlePart>().Title;
                        context.DocumentIndex.Add(term.Field, value).Analyze();
                        context.DocumentIndex.Add(term.Field + "-id", term.Id).Store();
                    }
                });
        }

        private static void RecalculateCount(IContentManager contentManager, ITaxonomyService taxonomyService, TermsPart part) {
            // submits any change to the db so that GetContentItemsCount is accurate
            contentManager.Flush();

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