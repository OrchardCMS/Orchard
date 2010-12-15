using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment;
using Orchard.Tasks;

namespace Orchard.Blogs.Routing {
    [UsedImplicitly]
    public class BlogSlugConstraintUpdator : IOrchardShellEvents, IBackgroundTask {
        private readonly IBlogSlugConstraint _blogSlugConstraint;
        private readonly IBlogService _blogService;

        public BlogSlugConstraintUpdator(IBlogSlugConstraint blogSlugConstraint, IBlogService blogService) {
            _blogSlugConstraint = blogSlugConstraint;
            _blogService = blogService;
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
            _blogSlugConstraint.SetSlugs(_blogService.Get().Select(b => b.As<IRoutableAspect>().Path));
        }
    }
}