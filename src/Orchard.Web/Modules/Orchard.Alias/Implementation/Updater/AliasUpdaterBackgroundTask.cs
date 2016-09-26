using Orchard.Environment.Extensions;
using Orchard.Tasks;

namespace Orchard.Alias.Implementation.Updater {
    [OrchardFeature("Orchard.Alias.Updater")]
    public class AliasUpdaterBackgroundTask : IBackgroundTask {

        private readonly IAliasHolderUpdater _aliasHolderUpdater;

        public AliasUpdaterBackgroundTask(IAliasHolderUpdater aliasHolderUpdater) {
            _aliasHolderUpdater = aliasHolderUpdater;
        }

        public void Sweep() {
            _aliasHolderUpdater.Refresh();
        }
    }

}