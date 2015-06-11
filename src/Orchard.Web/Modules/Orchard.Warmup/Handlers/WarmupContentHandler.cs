using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.Warmup.Models;
using Orchard.Warmup.Services;

namespace Orchard.Warmup.Handlers {
    /// <summary>
    /// Intercepts the ContentHandler events to create warmup static pages
    /// whenever some content is published
    /// </summary>
    public class WarmupContentHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IWarmupScheduler _warmupScheduler;

        public WarmupContentHandler(IOrchardServices orchardServices, IWarmupScheduler warmupScheduler)
        {
            _orchardServices = orchardServices;
            _warmupScheduler = warmupScheduler;

            OnPublished<ContentPart>(Generate);
        }

        void Generate(PublishContentContext context, ContentPart part) {
            if(_orchardServices.WorkContext.CurrentSite.As<WarmupSettingsPart>().OnPublish) {
                _warmupScheduler.Schedule(true);
            }
        }
    }
}
