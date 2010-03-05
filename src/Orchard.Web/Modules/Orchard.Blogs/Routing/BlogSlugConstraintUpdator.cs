using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Services;
using Orchard.Extensions;
using Orchard.Tasks;

namespace Orchard.Blogs.Routing {
    [UsedImplicitly]
    public class BlogSlugConstraintUpdator : ExtensionManagerEvents, IBackgroundTask {
        private readonly IBlogSlugConstraint _blogSlugConstraint;
        private readonly IBlogService _blogService;

        public BlogSlugConstraintUpdator(IBlogSlugConstraint blogSlugConstraint, IBlogService blogService) {
            _blogSlugConstraint = blogSlugConstraint;
            _blogService = blogService;
        }

        public override void Activated(ExtensionEventContext context) {
            if (context.Extension.Descriptor.Name == "Orchard.Blogs") {
                Refresh();
            }
        }

        public void Sweep() {
            Refresh();
        }

        private void Refresh() {
            _blogSlugConstraint.SetSlugs(_blogService.Get().Select(b => b.Slug));
        }
    }
}