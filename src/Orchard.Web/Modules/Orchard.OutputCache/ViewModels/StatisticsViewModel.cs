using System.Collections.Generic;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.ViewModels {
    public class StatisticsViewModel {
        public IEnumerable<CacheItem> CacheItems { get; set; }
        public dynamic Pager { get; set; }
    }
}