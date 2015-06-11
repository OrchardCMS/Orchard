using Orchard.ContentManagement;
using Orchard.Tasks;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public class WarmupTask : IBackgroundTask {
        private readonly IOrchardServices _orchardServices;
        private readonly IWarmupUpdater _warmupUpdater;

        public WarmupTask(IOrchardServices orchardServices, IWarmupUpdater warmupUpdater) {
            _orchardServices = orchardServices;
            _warmupUpdater = warmupUpdater;
        }

        public void Sweep() {
            var part = _orchardServices.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            if (!part.Scheduled) {
                return;
            }

            _warmupUpdater.EnsureGenerate();
        }
    }
}