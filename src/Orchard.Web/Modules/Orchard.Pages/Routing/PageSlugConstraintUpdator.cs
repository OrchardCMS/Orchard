using System.Linq;
using JetBrains.Annotations;
using Orchard.Extensions;
using Orchard.Pages.Services;
using Orchard.Tasks;

namespace Orchard.Pages.Routing {
    [UsedImplicitly]
    public class PageSlugConstraintUpdator : ExtensionManagerEvents, IBackgroundTask {
        private readonly IPageSlugConstraint _pageSlugConstraint;
        private readonly IPageService _pageService;

        public PageSlugConstraintUpdator(IPageSlugConstraint pageSlugConstraint, IPageService pageService) {
            _pageSlugConstraint = pageSlugConstraint;
            _pageService = pageService;
        }

        public override void Activated(ExtensionEventContext context) {
            if (context.Extension.Descriptor.Name == "Orchard.Pages") {
                Refresh();
            }
        }

        public void Sweep() {
            Refresh();
        }

        private void Refresh() {
            _pageSlugConstraint.SetSlugs(_pageService.Get(PageStatus.Published).Select(p => p.Slug));
        }
    }
}