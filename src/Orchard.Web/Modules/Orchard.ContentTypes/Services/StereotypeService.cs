using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Services;

namespace Orchard.ContentTypes.Services {
    public interface IStereotypeService : IDependency {
        IEnumerable<StereotypeDescription> GetStereotypes();
    }

    public class StereotypeService : IStereotypeService {
        private readonly IEnumerable<IStereotypesProvider> _providers;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IClock _clock;

        public StereotypeService(IEnumerable<IStereotypesProvider> providers, ICacheManager cacheManager, ISignals signals, IClock clock) {
            _providers = providers;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
        }

        public IEnumerable<StereotypeDescription> GetStereotypes() {
            return _cacheManager.Get("ContentType.Stereotypes", true, context => {

                // TODO: Implement a signal in ContentDefinitionManager that gets raised whenever a type definition is updated.
                // For now, we'll just cache the stereotypes for 1 minute.
                //context.Monitor(_signals.When("ContentType.Stereotypes"));
                context.Monitor(_clock.WhenUtc(_clock.UtcNow.AddMinutes(1)));
                return _providers.SelectMany(x => x.GetStereotypes());
            });
            
        }
    }
}