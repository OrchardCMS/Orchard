using IDeliverable.Widgets.Models;
using Orchard.Caching;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace IDeliverable.Widgets.Handlers
{
    [OrchardFeature("IDeliverable.Widgets.OutputCache")]
    public class OutputCachePartHandler : ContentHandler
    {
        private readonly ISignals _signals;

        public OutputCachePartHandler(ISignals signals)
        {
            _signals = signals;
            OnUpdated<OutputCachePart>(EvictCache);
        }

        private void EvictCache(UpdateContentContext context, OutputCachePart part)
        {
            _signals.Trigger(OutputCachePart.ContentSignalName(part.Id));
        }
    }
}