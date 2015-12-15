using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Taxonomies.Models;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.State;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;

namespace Orchard.Taxonomies.Services {
    public class TaxonomyService : ITaxonomyService {
        private readonly IRepository<TermContentItem> _termContentItemRepository;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _services;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        private readonly HashSet<int> _processedTermParts = new HashSet<int>(); 

        public TaxonomyService(
            IRepository<TermContentItem> termContentItemRepository,
            IContentManager contentManager,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IOrchardServices services,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager) {
            _termContentItemRepository = termContentItemRepository;
            _contentManager = contentManager;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _services = services;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IEnumerable<TaxonomyPart> GetTaxonomies() {
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>().List();
        }

        public TaxonomyPart GetTaxonomy(int id) {
            return _contentManager.Get(id, VersionOptions.Published).As<TaxonomyPart>();
        }

        public TaxonomyPart GetTaxonomyByName(string name) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }

            // include the record in the query to optimize the query plan
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        public TaxonomyPart GetTaxonomyBySlug(string slug) {
            if (String.IsNullOrWhiteSpace(slug)) {
                throw new ArgumentNullException("slug");
            }

            return _contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<TitlePartRecord>()
                .Join<AutoroutePartRecord>()
                .Where(r => r.DisplayAlias == slug)
                .List()
                .FirstOrDefault();
        }

        public void CreateTermContentType(TaxonomyPart taxonomy) {
            // create the associated term's content type
            taxonomy.TermTypeName = GenerateTermTypeName(taxonomy.Name);

            _contentDefinitionManager.AlterTypeDefinition(taxonomy.TermTypeName,
                cfg => cfg
                    .WithSetting("Taxonomy", taxonomy.Name)
                    .WithPart("TermPart")
                    .WithPart("TitlePart")
                    .WithPart("AutoroutePart", builder => builder
                        .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                        .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                        .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Taxonomy and Title', Pattern: '{Content.Container.Path}/{Content.Slug}', Description: 'my-taxonomy/my-term/sub-term'}]"))
                    .WithPart("CommonPart")
                    .DisplayedAs(taxonomy.Name + " Term")
                );

        }

        public void DeleteTaxonomy(TaxonomyPart taxonomy) {
            _contentManager.Remove(taxonomy.ContentItem);

            // Removing terms
            foreach (var term in GetTerms(taxonomy.Id)) {
                DeleteTerm(term);
            }

            _contentDefinitionManager.DeleteTypeDefinition(taxonomy.TermTypeName);
        }

        public string GenerateTermTypeName(string taxonomyName) {
            var name = taxonomyName.ToSafeName() + "Term";
            int i = 2;
            while (_contentDefinitionManager.GetTypeDefinition(name) != null) {
                name = taxonomyName.ToSafeName() + i++;
            }

            return name;
        }

        public TermPart NewTerm(TaxonomyPart taxonomy) {
            return NewTerm(taxonomy, null);
        }

        public TermPart NewTerm(TaxonomyPart taxonomy, IContent parent) {
            if (taxonomy == null) {
                throw new ArgumentNullException("taxonomy");
            }

            if (parent != null) {
                var parentAsTaxonomy = parent.As<TaxonomyPart>();
                if (parentAsTaxonomy != null && parentAsTaxonomy != taxonomy) {
                    throw new ArgumentException("The parent of a term can't be a different taxonomy", "parent");
                }

                var parentAsTerm = parent.As<TermPart>();
                if (parentAsTerm != null && parentAsTerm.TaxonomyId != taxonomy.Id) {
                    throw new ArgumentException("The parent of a term can't be a from a different taxonomy", "parent");
                }
            }

            var term = _contentManager.New<TermPart>(taxonomy.TermTypeName);
            term.Container = parent ?? taxonomy;
            term.TaxonomyId = taxonomy.Id;
            ProcessPath(term);

            return term;
        }

        public IEnumerable<TermPart> GetTerms(int taxonomyId) {
            var result = _contentManager.Query<TermPart, TermPartRecord>()
                .Where(x => x.TaxonomyId == taxonomyId)
                .List();

            return TermPart.Sort(result);
        }

        public TermPart GetTermByPath(string path) {
            return _contentManager.Query<TermPart, TermPartRecord>()
                .Join<AutoroutePartRecord>()
                .Where(rr => rr.DisplayAlias == path)
                .List()
                .FirstOrDefault();
        }

        public IEnumerable<TermPart> GetAllTerms() {
            var result = _contentManager
                .Query<TermPart, TermPartRecord>()
                .List();
            return TermPart.Sort(result);
        }

        public int GetTermsCount(int taxonomyId) {
            return _contentManager.Query<TermPart, TermPartRecord>()
                .Where(x => x.TaxonomyId == taxonomyId)
                .Count();
        }

        public TermPart GetTerm(int id) {
            return _contentManager
                .Query<TermPart, TermPartRecord>()
                .Where(x => x.Id == id).List().FirstOrDefault();
        }

        public IEnumerable<TermPart> GetTermsForContentItem(int contentItemId, string field = null, VersionOptions versionOptions = null) {
            var termIds = String.IsNullOrEmpty(field)
                ? _termContentItemRepository.Fetch(x => x.TermsPartRecord.ContentItemRecord.Id == contentItemId).Select(t => t.TermRecord.Id).ToArray()
                : _termContentItemRepository.Fetch(x => x.TermsPartRecord.Id == contentItemId && x.Field == field).Select(t => t.TermRecord.Id).ToArray();

            return _contentManager.GetMany<TermPart>(termIds, versionOptions ?? VersionOptions.Published, QueryHints.Empty);
        }

        public TermPart GetTermByName(int taxonomyId, string name) {
            return _contentManager
                .Query<TermPart, TermPartRecord>()
                .Where(t => t.TaxonomyId == taxonomyId)
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        public void CreateTerm(TermPart termPart) {
            if (GetTermByName(termPart.TaxonomyId, termPart.Name) == null) {
                _authorizationService.CheckAccess(Permissions.CreateTerm, _services.WorkContext.CurrentUser, null);

                termPart.As<ICommonPart>().Container = GetTaxonomy(termPart.TaxonomyId).ContentItem;
                _contentManager.Create(termPart);
            } else {
                _notifier.Warning(T("The term {0} already exists in this taxonomy", termPart.Name));
            }
        }

        public void DeleteTerm(TermPart termPart) {
            _contentManager.Remove(termPart.ContentItem);

            foreach (var childTerm in GetChildren(termPart)) {
                _contentManager.Remove(childTerm.ContentItem);
            }

            // delete termContentItems
            var termContentItems = _termContentItemRepository
                .Fetch(t => t.TermRecord == termPart.Record)
                .ToList();

            foreach (var termContentItem in termContentItems) {
                _termContentItemRepository.Delete(termContentItem);
            }
        }

        public void UpdateTerms(ContentItem contentItem, IEnumerable<TermPart> terms, string field) {
            var termsPart = contentItem.As<TermsPart>();

            // removing current terms for specific field
            var termList = termsPart.Terms.Select((t, i) => new { Term = t, Index = i })
                .Where(x => x.Term.Field == field)
                .Select(x => x)
                .OrderByDescending(i => i.Index)
                .ToList();

            foreach (var x in termList) {
                termsPart.Terms.RemoveAt(x.Index);
            }

            // adding new terms list
            foreach (var term in terms) {
                // Remove the newly added terms because they will get processed by the Published-Event
                termList.RemoveAll(t => t.Term.Id == term.Id);
                termsPart.Terms.Add(
                    new TermContentItem {
                        TermsPartRecord = termsPart.Record,
                        TermRecord = term.Record,
                        Field = field
                    });
            }

            var termPartRecordIds = termList.Select(t => t.Term.TermRecord.Id).ToArray();
            if (termPartRecordIds.Any()) {
                if (!_processedTermParts.Any()) {
                    _processingEngine.AddTask(_shellSettings, _shellDescriptorManager.GetShellDescriptor(), "ITermCountProcessor.Process", new Dictionary<string, object> { { "termPartRecordIds", _processedTermParts } });
                }
                foreach (var termPartRecordId in termPartRecordIds) {
                    _processedTermParts.Add(termPartRecordId);                    
                }
            }
        }

        public IContentQuery<TermsPart, TermsPartRecord> GetContentItemsQuery(TermPart term, string fieldName = null) {
            var rootPath = term.FullPath + "/";

            var query = _contentManager
                .Query<TermsPart, TermsPartRecord>();

            if (String.IsNullOrWhiteSpace(fieldName)) {
                query = query.Where(
                    tpr => tpr.Terms.Any(tr =>
                        tr.TermRecord.Id == term.Id
                        || tr.TermRecord.Path.StartsWith(rootPath)));
            } else {
                query = query.Where(
                    tpr => tpr.Terms.Any(tr =>
                        tr.Field == fieldName
                         && (tr.TermRecord.Id == term.Id || tr.TermRecord.Path.StartsWith(rootPath))));
            }

            return query;
        }

        public long GetContentItemsCount(TermPart term, string fieldName = null) {
            return GetContentItemsQuery(term, fieldName).Count();
        }

        public IEnumerable<IContent> GetContentItems(TermPart term, int skip = 0, int count = 0, string fieldName = null) {
            return GetContentItemsQuery(term, fieldName)
                .Join<CommonPartRecord>()
                .OrderByDescending(x => x.CreatedUtc)
                .Slice(skip, count);
        }

        public IEnumerable<TermPart> GetChildren(TermPart term) {
            return GetChildren(term, false);
        }

        public IEnumerable<TermPart> GetChildren(TermPart term, bool includeParent) {
            var rootPath = term.FullPath + "/";

            var result = _contentManager.Query<TermPart, TermPartRecord>()
                .Where(x => x.Path.StartsWith(rootPath))
                .List();

            if (includeParent) {
                result = result.Concat(new[] { term });
            }

            return TermPart.Sort(result);
        }

        public IEnumerable<TermPart> GetParents(TermPart term) {
            return term.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Select(id => GetTerm(int.Parse(id)));
        }

        public IEnumerable<string> GetSlugs() {
            return _contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>()
                .List()
                .Select(t => t.Slug);
        }

        public IEnumerable<string> GetTermPaths() {
            return _contentManager
                .Query<TermPart, TermPartRecord>()
                .List()
                .Select(t => t.Slug);
        }

        public void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm) {
            var children = GetChildren(term);
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;
            ProcessPath(term);

            var contentItem = _contentManager.Get(term.ContentItem.Id, VersionOptions.DraftRequired);
            _contentManager.Publish(contentItem);

            foreach (var childTerm in children) {
                ProcessPath(childTerm);

                contentItem = _contentManager.Get(childTerm.ContentItem.Id, VersionOptions.DraftRequired);
                _contentManager.Publish(contentItem);
            }
        }

        public void ProcessPath(TermPart term) {
            var parentTerm = term.Container.As<TermPart>();
            term.Path = parentTerm != null ? parentTerm.FullPath + "/" : "/";
        }

        public void CreateHierarchy(IEnumerable<TermPart> terms, Action<TermPartNode, TermPartNode> append) {
            var root = new TermPartNode();
            var stack = new Stack<TermPartNode>(new[] { root });

            foreach (var term in terms) {
                var current = CreateNode(term);
                var previous = stack.Pop();

                while (previous.Level + 1 != current.Level) {
                    previous = stack.Pop();
                }

                if (append != null) {
                    append(previous, current);
                }

                previous.Items.Add(current);
                current.Parent = previous;

                stack.Push(previous);
                stack.Push(current);
            }
        }

        private static TermPartNode CreateNode(TermPart part) {
            return new TermPartNode {
                TermPart = part,
                Level = part.Path.Count(x => x == '/')
            };
        }

        public IContentQuery<TaxonomyPart, TaxonomyPartRecord> GetTaxonomiesQuery() {
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>();
        }

        public IContentQuery<TermPart, TermPartRecord> GetTermsQuery(int taxonomyId) {
            return _contentManager.Query<TermPart, TermPartRecord>().Where(x => x.TaxonomyId == taxonomyId);
        }
    }
}
