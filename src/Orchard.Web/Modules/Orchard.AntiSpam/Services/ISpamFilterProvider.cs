using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.AntiSpam.Services {
    public interface ISpamFilterProvider : IDependency {
        IEnumerable<ISpamFilter> GetSpamFilters();
    }
}
