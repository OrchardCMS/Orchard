using Orchard.Events;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Events {
    public interface ITermLocalizationEventHandler : IEventHandler {
        void MovingTerms(MoveTermsContext context);
    }
}