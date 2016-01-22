using System.Collections.Generic;

namespace Orchard.AntiSpam.Services {
    /// <summary>
    /// Implements <see cref="ISpamFilterProvider"/> by returning Akismet and TypePad services if they are configured
    /// </summary>
    public class DefaultSpamFilterProvider : ISpamFilterProvider {
        public IEnumerable<ISpamFilter> GetSpamFilters() {
            yield break;
        }
    }
}
