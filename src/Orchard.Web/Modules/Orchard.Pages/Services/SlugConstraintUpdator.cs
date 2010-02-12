using System.Linq;
using Orchard.Extensions;
using Orchard.Tasks;

namespace Orchard.Pages.Services {
    public class SlugConstraintUpdator : ExtensionManagerEvents, IBackgroundTask {
        private readonly ISlugConstraint _slugConstraint;
        private readonly IPageService _pageService;

        public SlugConstraintUpdator(ISlugConstraint slugConstraint, IPageService pageService) {
            _slugConstraint = slugConstraint;
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
            _slugConstraint.SetCurrentlyPublishedSlugs(_pageService.Get(PageStatus.Published).Select(page => page.Slug));
        }
    }
}