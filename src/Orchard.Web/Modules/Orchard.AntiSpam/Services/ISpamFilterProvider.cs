using System.Collections.Generic;

namespace Orchard.AntiSpam.Services
{
    public interface ISpamFilterProvider : IDependency
    {
        IEnumerable<ISpamFilter> GetSpamFilters();
    }
}
