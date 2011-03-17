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
        private readonly IWarmupUpdater _warmupUpdater;

        public WarmupContentHandler(IOrchardServices orchardServices, IWarmupUpdater warmupUpdater) {
            _orchardServices = orchardServices;
            _warmupUpdater = warmupUpdater;

            OnPublished<ContentPart>(Generate);
        }

        void Generate(PublishContentContext context, ContentPart part) {
            if(_orchardServices.WorkContext.CurrentSite.As<WarmupSettingsPart>().OnPublish) {
                _warmupUpdater.Generate();
            }
        }
    }
}
