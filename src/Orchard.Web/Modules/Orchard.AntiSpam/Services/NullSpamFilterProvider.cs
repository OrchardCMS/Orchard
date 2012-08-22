using System.Collections.Generic;

namespace Orchard.AntiSpam.Services {
    /// <summary>
    /// Default implementation for <see cref="ISpamFilterProvider"/> in order to have at least one result
    /// when it is resolved, event if there is no other concrete provider.
    /// </summary>
    public class NullSpamFilterProvider : ISpamFilterProvider {
        public IEnumerable<ISpamFilter> GetSpamFilters() {
            yield break;
        }
    }
}
