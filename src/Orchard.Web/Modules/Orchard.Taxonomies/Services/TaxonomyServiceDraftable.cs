using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
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
        
        public override TaxonomyPart GetTaxonomy(int id) {
            return _contentManager.Get(id, VersionOptions.Latest).As<TaxonomyPart>();
        }
        
        public override IContentQuery<TaxonomyPart, TaxonomyPartRecord> GetTaxonomiesQuery() {
            return base.GetTaxonomiesQuery().ForVersion(VersionOptions.Latest);
        }

        public override IContentQuery<TermPart, TermPartRecord> GetTermsQuery() {
            return base.GetTermsQuery().ForVersion(VersionOptions.Latest);
        }

        protected override void PublishTerm(TermPart term) {
            // Only publish the Term if it was published already.
            if (term.ContentItem.HasPublished() && !term.ContentItem.IsPublished()) {
                var contentItem = _contentManager.Get(term.ContentItem.Id, VersionOptions.DraftRequired);
                _contentManager.Publish(contentItem);
            }
        }
    }
}
