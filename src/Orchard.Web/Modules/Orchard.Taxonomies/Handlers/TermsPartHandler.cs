using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Utilities;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.State;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;

namespace Orchard.Taxonomies.Handlers {
    public class TermsPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        
        private readonly HashSet<int> _processedTermParts = new HashSet<int>(); 

        public TermsPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            IRepository<TermsPartRecord> repository,
            ITaxonomyService taxonomyService,
            IContentManager contentManager,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));
            OnPublished<TermsPart>((context, part) => RecalculateCount(processingEngine, shellSettings, shellDescriptorManager, part));
            OnUnpublished<TermsPart>((context, part) => RecalculateCount(processingEngine, shellSettings, shellDescriptorManager, part));
            OnRemoved<TermsPart>((context, part) => RecalculateCount(processingEngine, shellSettings, shellDescriptorManager, part));

            // Tells how to load the field terms on demand, when a content item it loaded or when it has been created
            OnInitialized<TermsPart>((context, part) => InitializerTermsLoader(part));
            OnLoading<TermsPart>((context, part) => InitializerTermsLoader(part));
            OnUpdating<TermsPart>((context, part) => InitializerTermsLoader(part));

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

        private void InitializerTermsLoader(TermsPart part) {
            if (part._termParts != null) {
                return;
            }

            var queryHint = new QueryHints()
                .ExpandRecords("ContentTypeRecord", "CommonPartRecord", "TermsPartRecord");


            foreach (var field in part.ContentItem.Parts.SelectMany(p => p.Fields).OfType<TaxonomyField>()) {
                var tempField = field.Name;
                field.TermsField.Loader(() => {
                    var fieldTermRecordIds = part.Record.Terms.Where(t => t.Field == tempField).Select(tci => tci.TermRecord.Id);
                    var terms = _contentManager.GetMany<TermPart>(fieldTermRecordIds, VersionOptions.Published, queryHint);
                    return terms.ToList();
                });
            }

            part._termParts = new LazyField<IEnumerable<TermContentItemPart>>();
            part._termParts.Loader(() => {
                var ids = part.Terms.Select(t => t.TermRecord.Id).Distinct();
                var terms = _contentManager.GetMany<TermPart>(ids, VersionOptions.Published, queryHint)
                    .ToDictionary(t => t.Id, t => t);
                return
                    part.Terms.Select(
                        x =>
                            new TermContentItemPart {
                                Field = x.Field,
                                TermPart = terms[x.TermRecord.Id]
                            }
                        );
            });
        }

                // Fires off a processing engine task to run the count processing after the request so it's non-blocking.
        private void RecalculateCount(IProcessingEngine processingEngine, ShellSettings shellSettings, IShellDescriptorManager shellDescriptorManager, TermsPart part) {
            var termPartRecordIds = part.Terms.Select(t => t.TermRecord.Id).ToArray();
            if (termPartRecordIds.Any()) {
                if (!_processedTermParts.Any()) {
                    processingEngine.AddTask(shellSettings, shellDescriptorManager.GetShellDescriptor(), "ITermCountProcessor.Process", new Dictionary<string, object> { { "termPartRecordIds", _processedTermParts } });
                }
                foreach (var termPartRecordId in termPartRecordIds) {
                    _processedTermParts.Add(termPartRecordId);                    
                }
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
