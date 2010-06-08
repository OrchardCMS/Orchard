using System.Linq;
using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Tasks;

namespace Orchard.Core.Routable.Services {
    [UsedImplicitly]
    public class RoutablePathConstraintUpdator : IOrchardShellEvents, IBackgroundTask {
        private readonly IRoutablePathConstraint _pageSlugConstraint;
        private readonly IRepository<RoutableRecord> _repository;

        public RoutablePathConstraintUpdator(IRoutablePathConstraint pageSlugConstraint, IRepository<RoutableRecord> repository) {
            _pageSlugConstraint = pageSlugConstraint;
            _repository = repository;
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
            var slugs = _repository.Fetch(r => r.ContentItemVersionRecord.Published && r.Path != "" && r.Path != null).Select(r => r.Path);

            _pageSlugConstraint.SetPaths(slugs);
        }
    }
}
