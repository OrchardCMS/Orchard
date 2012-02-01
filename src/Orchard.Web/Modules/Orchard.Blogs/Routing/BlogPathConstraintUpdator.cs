using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment;
using Orchard.Tasks;

namespace Orchard.Blogs.Routing {
    [UsedImplicitly]
    public class BlogPathConstraintUpdator : IOrchardShellEvents, IBackgroundTask {
        private readonly IBlogPathConstraint _blogPathConstraint;
        private readonly IBlogService _blogService;

        public BlogPathConstraintUpdator(IBlogPathConstraint blogPathConstraint, IBlogService blogService) {
            _blogPathConstraint = blogPathConstraint;
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
            _blogPathConstraint.SetPaths(_blogService.Get().Select(b => b.As<IAliasAspect>().Path).ToList());
        }
    }
}