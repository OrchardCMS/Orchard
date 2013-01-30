using Contrib.Taxonomies.Services;
using Orchard;
using Orchard.Environment;

namespace Contrib.Taxonomies.Routing {
    public interface ITaxonomySlugConstraintUpdator : IDependency {
        void Refresh();
    }

    public class TaxonomySlugConstraintUpdator : ITaxonomySlugConstraintUpdator, IOrchardShellEvents {
        private readonly ITaxonomySlugConstraint _taxonomySlugConstraint;
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomySlugConstraintUpdator(ITaxonomySlugConstraint taxonomySlugConstraint, ITaxonomyService taxonomyService) {
            _taxonomySlugConstraint = taxonomySlugConstraint;
            _taxonomyService = taxonomyService;
        }
        
        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        public void Refresh() {
            _taxonomySlugConstraint.SetSlugs(_taxonomyService.GetSlugs());
        }
    }
}