using System;
using System.Collections.Generic;
using System.Linq;
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
using Orchard.Taxonomies.Models;
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

        private readonly HashSet<int> _processedTermPartIds = new HashSet<int>();

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
            return GetTaxonomiesQuery().List();
        }

        public virtual TaxonomyPart GetTaxonomy(int id) {
            return _contentManager.Get(id, VersionOptions.Published).As<TaxonomyPart>();
        }

        public TaxonomyPart GetTaxonomyByName(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }

            // include the record in the query to optimize the query plan
            return GetTaxonomiesQuery()
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        public TaxonomyPart GetTaxonomyBySlug(string slug) {
            if (string.IsNullOrWhiteSpace(slug)) {
                throw new ArgumentNullException("slug");
            }

            return GetTaxonomiesQuery()
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
            List<TermPart> allTerms = GetRootTerms(taxonomy.Id).ToList();
            // Removing terms
            foreach (var term in allTerms) {
                DeleteTerm(term);
            }

            if (_contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>()
                .Where(x => x.Id != taxonomy.Id && x.TermTypeName == taxonomy.TermTypeName)
                .Count() == 0) {

                _contentDefinitionManager.DeleteTypeDefinition(taxonomy.TermTypeName);
            }
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
            var result = GetTermsQuery(taxonomyId)
                .OrderBy(x => x.FullWeight)
                .List();

            return result;
        }

        public IEnumerable<TermPart> GetRootTerms(int taxonomyId) {
            var result = GetTermsQuery(taxonomyId)
                .Where(x => x.Path == "/")
                .OrderBy(x => x.FullWeight)
                .List();

            return result;
        }

        public TermPart GetTermByPath(string path) {
            return GetTermsQuery()
                .Join<AutoroutePartRecord>()
                .Where(rr => rr.DisplayAlias == path)
                .List()
                .FirstOrDefault();
        }

        public IEnumerable<TermPart> GetAllTerms() {
            var result = GetTermsQuery()
                .OrderBy(x => x.TaxonomyId)
                .OrderBy(x => x.FullWeight)
                .List();
            return result;
        }

        public int GetTermsCount(int taxonomyId) {
            return GetTermsQuery(taxonomyId)
                .Count();
        }

        public TermPart GetTerm(int id) {
            return GetTermsQuery()
                .Where(x => x.Id == id).List().FirstOrDefault();
        }

        public IEnumerable<TermPart> GetTermsForContentItem(
            int contentItemId, string field = null, VersionOptions versionOptions = null) {

            var termIds = string.IsNullOrEmpty(field)
                ? _termContentItemRepository
                    .Table
                    .Where(x => x.TermsPartRecord.ContentItemRecord.Id == contentItemId)
                    .Select(t => t.TermRecord.Id)
                    .ToArray()
                : _termContentItemRepository
                    .Table
                    .Where(x => x.TermsPartRecord.Id == contentItemId && x.Field == field)
                    .Select(t => t.TermRecord.Id)
                    .ToArray();

            return _contentManager
                .GetMany<TermPart>(termIds, versionOptions ?? VersionOptions.Published, QueryHints.Empty);
        }

        public TermPart GetTermByName(int taxonomyId, string name) {
            return GetTermsQuery(taxonomyId)
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
                termList.RemoveAll(t => t.Term.TermRecord.Id == term.Id);
                termsPart.Terms.Add(
                    new TermContentItem {
                        TermsPartRecord = termsPart.Record,
                        TermRecord = term.Record,
                        Field = field
                    });
            }

            var termPartRecordIds = termList.Select(t => t.Term.TermRecord.Id).ToArray();
            if (termPartRecordIds.Any()) {
                if (!_processedTermPartIds.Any()) {
                    _processingEngine.AddTask(_shellSettings, _shellDescriptorManager.GetShellDescriptor(), "ITermCountProcessor.Process", new Dictionary<string, object> { { "termPartRecordIds", _processedTermPartIds } });
                }
                foreach (var termPartRecordId in termPartRecordIds) {
                    _processedTermPartIds.Add(termPartRecordId);
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

            var result = GetTermsQuery()
                .Where(x => x.Path.StartsWith(rootPath))
                .OrderBy(x => x.FullWeight)
                .List();

            if (includeParent) {
                result = result.Concat(new[] { term });
            }

            return result;
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
            return GetTermsQuery()
                .List()
                .Select(t => t.Slug);
        }

        public void MoveTerm(TaxonomyPart taxonomy, TermPart term, TermPart parentTerm) {
            // get the children before changing the path
            var children = GetChildren(term);
            // compute new path and publish. This also computes the new weight and
            // recursively does the same for siblings and children of the TermPart
            // that was moved.
            // In case we are changing the taxonomy, we will have to properly evict
            // caches
            if (taxonomy.Id != term.TaxonomyId) {
                if (_termFamilies == null) {
                    _termFamilies = new Dictionary<string, IEnumerable<TermPart>>();
                } else {
                    // evict the old cache for the term and all children
                    var oldKey = term.TaxonomyId.ToString() + (term.Path ?? string.Empty);
                    if (_termFamilies.ContainsKey(oldKey)) {
                        _termFamilies.Remove(oldKey);
                    }
                    foreach (var child in children) {
                        oldKey = child.TaxonomyId.ToString() + (child.Path ?? string.Empty);
                        if (_termFamilies.ContainsKey(oldKey)) {
                            _termFamilies.Remove(oldKey);
                        }
                    }
                }
            }
            term.TaxonomyId = taxonomy.Id;
            term.Container = parentTerm == null ? taxonomy.ContentItem : parentTerm.ContentItem;
            ProcessPath(term);
            // process the children
            foreach (var child in children) {
                child.TaxonomyId = taxonomy.Id;
                ProcessPath(child);
            }
        }

        public void ProcessPath(TermPart term) {
            // if term.Path changes, we should remove it from the Dictionary we are
            // using to cache siblings, otherwise we process wrong TermParts
            var parentTerm = term.Container.As<TermPart>();
            var oldPath = term.Path ?? string.Empty;
            term.Path = parentTerm != null ? parentTerm.FullPath + "/" : "/";
            if (!oldPath.Equals(term.Path, StringComparison.InvariantCultureIgnoreCase)) {
                // path has changed. evict stale caches.
                if (_termFamilies == null) {
                    _termFamilies = new Dictionary<string, IEnumerable<TermPart>>();
                } else {
                    // We evict the cache for both the old and new values of the path.
                    // old: processing a former sibling we cause us to reprocess this term
                    //   when we don't need to.
                    if (_termFamilies.ContainsKey(term.TaxonomyId + oldPath)) {
                        _termFamilies.Remove(term.TaxonomyId + oldPath);
                    }
                    // new: attempting to process the weight for this term would skip it.
                    if (_termFamilies.ContainsKey(term.TaxonomyId + term.Path)) {
                        _termFamilies.Remove(term.TaxonomyId + term.Path);
                    }
                }
            }
            // oldPath is empty when creating a new term, before doing all the updates
            if (!string.IsNullOrWhiteSpace(oldPath)) {
                // Reprocess weights
                ProcessFullWeight(term);
            }
        }
        private void ProcessFullWeight(TermPart term) {
            // Given part
            // - Update FullWeight for term
            // - Update FullWeight for term's siblings of the same weight
            // - If term's FullWeight changed, update path and FullWeight for all its children
            // - If FullWeight changed for any of term's siblings, update their children
            // We don't have to check for each child's siblings, because we are updating
            // all children anyway (as long as we have to update any).

            // Get term and its siblings
            var litter = OrderedSiblings(term)
                .Where(sib => sib.Weight == term.Weight);

            // For each one, see whether we should update its weight.
            // in that case, update its children as well.
            // Note that term is included in that IEnumerable already, in its place among
            // its siblings, ordered by title alphabetically.
            foreach (var tp in litter) {
                var newWeight = ComputeFullWeight(tp);
                if (!newWeight.Equals(tp.FullWeight, StringComparison.InvariantCultureIgnoreCase)) {
                    tp.FullWeight = newWeight;
                    PublishTerm(tp);
                    UpdateChildren(tp);
                }
            }
        }
        /// <summary>
        /// This will update both path and weight for the children of the given TermPart.
        /// Then it will recursively update the children of each child.
        /// </summary>
        /// <param name="part"></param>
        private void UpdateChildren(TermPart part) {
            foreach (var childTerm in GetChildren(part)) {
                ProcessPath(childTerm);
            }
        }
        protected virtual void PublishTerm(TermPart term) {
            var contentItem = _contentManager.Get(term.ContentItem.Id, VersionOptions.DraftRequired);
            _contentManager.Publish(contentItem);
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

        public virtual IContentQuery<TaxonomyPart, TaxonomyPartRecord> GetTaxonomiesQuery() {
            return _contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>();
        }

        public virtual IContentQuery<TermPart, TermPartRecord> GetTermsQuery() {
            return _contentManager
                .Query<TermPart, TermPartRecord>();
        }

        public IContentQuery<TermPart, TermPartRecord> GetTermsQuery(int taxonomyId) {
            return GetTermsQuery()
                .Where(x => x.TaxonomyId == taxonomyId);
        }

        public string ComputeFullWeight(TermPart part) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            // A TermPart's FullWeight property should be a string that univocally
            // allows the tree-like ordering of all terms in a taxonomy. For a given
            // TermPart, it should include information on its weight and its name.
            // Another factor to account is the TermPart's path, or more precisely the
            // hierarchy of terms "above" it. The order of terms resulting from a
            // OrderBy on the FullWeight:
            //  - Parents come before children.
            //  - For terms at a same level, the ones with higher Weight come first.
            //  - For terms at the same level and with the same Weight, we use the
            //    alphabetical order of their titles.
            // Additional factors:
            //  - We don't know beforehand how many terms are there in each level of
            //    a taxonomy.
            //  - We don't know beforehand how deep a taxonomy can go.
            // These latter two factors mean we should consider ways to make the FullWeight
            // string shorter. To make the strings shorter, we use the hex representation
            // of integers; we set that to a fixed lenght so that the number of
            // characters does not affect our ordering.
            // (1048575).ToString("X5") = "FFFFF"
            // The maximum length of the string in the db poses an hard limit on the
            // number of characters for the FullWeight, hence on the number of levels
            // in the hierarchy.

            // parent comes before child, so we will append a child's weight to
            // its parent's
            var parent = part.Container.As<TermPart>();
            var parentWeight = parent == null
                ? string.Empty
                : parent.FullWeight;
            // descending order of assigned weight. A "normal" OrderBy in SQL gives
            // results sorted in ascending order. Terms in Orchard have always been
            // order by descending weight. This needs to be a fixed length.
            // Since weights may be negative, we bias them around the middle of the
            // valid range we are considering.
            var partWeight = (524288 - part.Weight).ToString("X5");
            // siblings weight: this is a "comparative" term to include alphabetical
            // ordering of the titles. This needs to be a fixed length, just like for
            // the string for the weight assigned to the part. This fixed length poses
            // an hard limit on the number of terms on the same level and with the same
            // weight that are allowed in a taxonomy.
            // Siblings in a taxonomy are those TermParts that have the same Path.
            // We are only interested in those with the same Weight.
            var siblingsIds = OrderedSiblings(part)
                .Where(sib => sib.Weight == part.Weight)
                .Select(tp => tp.Id)
                .ToArray();
            var siblingsWeight = (1048575).ToString("X5");
            for (int i = 0; i < siblingsIds.Length; i++) {
                if (siblingsIds[i] == part.Id) {
                    siblingsWeight = i.ToString("X5");
                    break;
                }
            }
            // the part's Id ensures that no two FullWeight strings will be the same.
            // This is the only variable length portion of a TermPart's own weight. We
            // should never rely on this component of the order for anything: its main
            // point is to make sure each string is unique.
            return $"{parentWeight}{partWeight}.{siblingsWeight}.{part.Id}/";
            // The length of the portion of weight for a given term is
            // 5 + 1 + 5 + 1 + x + 1 = x + 13, where x is the number of characters of the Id.
        }

        private Dictionary<string, IEnumerable<TermPart>> _termFamilies;
        public IEnumerable<TermPart> OrderedSiblings(TermPart part) {
            if (_termFamilies == null) {
                _termFamilies = new Dictionary<string, IEnumerable<TermPart>>();
            }
            var key = part.TaxonomyId + part.Path;
            if (!_termFamilies.ContainsKey(key)) {
                _termFamilies.Add(key, GetTermsQuery(part.TaxonomyId)
                    .Where(tpr => tpr.Path == part.Path)
                    .List()
                    // we are ordering in memory to use the StringComparer that used
                    // to be used when sorting TermParts
                    .OrderBy(tp => tp.Name, StringComparer.OrdinalIgnoreCase));
            }
            return _termFamilies[key];
        }
    }
}
