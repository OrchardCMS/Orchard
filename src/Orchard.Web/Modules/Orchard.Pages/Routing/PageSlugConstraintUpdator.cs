using System.Linq;
using JetBrains.Annotations;
using Orchard.Environment;
using Orchard.Pages.Services;
using Orchard.Tasks;

namespace Orchard.Pages.Routing {
    [UsedImplicitly]
    public class PageSlugConstraintUpdator : IOrchardShellEvents, IBackgroundTask {
        private readonly IPageSlugConstraint _pageSlugConstraint;
        private readonly IPageService _pageService;

        public PageSlugConstraintUpdator(IPageSlugConstraint pageSlugConstraint, IPageService pageService) {
            _pageSlugConstraint = pageSlugConstraint;
            _pageService = pageService;
        }

        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        void IBackgroundTask.Sweep() {
            Refresh();
        }

        private void Refresh() {
            _pageSlugConstraint.SetSlugs(_pageService.Get(PageStatus.Published).Select(p => p.Slug));
        }

    }
}