using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Extensions;
using Orchard.Environment.State;
using Orchard.Security;
using Orchard.Taxonomies.Models;
using Orchard.UI.Notify;

namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyServiceDraftable : TaxonomyService, ITaxonomyService {
        private readonly IRepository<TermContentItem> _termContentItemRepository;
        private readonly IContentManager _contentManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _services;
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public TaxonomyServiceDraftable(
            IRepository<TermContentItem> termContentItemRepository,
            IContentManager contentManager,
            INotifier notifier,
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IOrchardServices services,
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager) : base(
                termContentItemRepository,
                contentManager,
                notifier,
                contentDefinitionManager,
                authorizationService,
                services,
                processingEngine,
                shellSettings,
                shellDescriptorManager) {
            _termContentItemRepository = termContentItemRepository;
            _contentManager = contentManager;
            _notifier = notifier;
            _authorizationService = authorizationService;
            _contentDefinitionManager = contentDefinitionManager;
            _services = services;
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
        }

        new public IEnumerable<TaxonomyPart> GetTaxonomies() {
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>().ForVersion(VersionOptions.Latest).List();
        }

        new public TaxonomyPart GetTaxonomy(int id) {
            return _contentManager.Get(id, VersionOptions.Latest).As<TaxonomyPart>();
        }

        new public TaxonomyPart GetTaxonomyByName(string name) {

            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }

            // include the record in the query to optimize the query plan
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>().ForVersion(VersionOptions.Latest)
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        new public TaxonomyPart GetTaxonomyBySlug(string slug) {
            if (String.IsNullOrWhiteSpace(slug)) {
                throw new ArgumentNullException("slug");
            }

            return _contentManager
                .Query<TaxonomyPart, TaxonomyPartRecord>().ForVersion(VersionOptions.Latest)
                .Join<TitlePartRecord>()
                .Join<AutoroutePartRecord>()
                .Where(r => r.DisplayAlias == slug)
                .List()
                .FirstOrDefault();
        }

        new public IEnumerable<TermPart> GetTerms(int taxonomyId) {
            var result = _contentManager.Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest)
                .Where(x => x.TaxonomyId == taxonomyId)
                .OrderBy(x=>x.FullWeight)
                .List();

            return result;
        }

        new public TermPart GetTermByPath(string path) {
            return _contentManager.Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest)
                .Join<AutoroutePartRecord>()
                .Where(rr => rr.DisplayAlias == path)
                .List()
                .FirstOrDefault();
        }

        new public IEnumerable<TermPart> GetAllTerms() {
            var result = _contentManager
                .Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest)
                .OrderBy(x=>x.TaxonomyId)
                .OrderBy(x=>x.FullWeight)
                .List();
            return result;
        }

        new public TermPart GetTerm(int id) {
            return _contentManager
                .Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest)
                .Where(x => x.Id == id).List().FirstOrDefault();
        }

        new public TermPart GetTermByName(int taxonomyId, string name) {
            return _contentManager
                .Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest)
                .Where(t => t.TaxonomyId == taxonomyId)
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
        }

        new public IContentQuery<TaxonomyPart, TaxonomyPartRecord> GetTaxonomiesQuery() {
            return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>().ForVersion(VersionOptions.Latest);
        }

        new public IContentQuery<TermPart, TermPartRecord> GetTermsQuery(int taxonomyId) {
            return _contentManager.Query<TermPart, TermPartRecord>().ForVersion(VersionOptions.Latest).Where(x => x.TaxonomyId == taxonomyId);
        }
    }
}