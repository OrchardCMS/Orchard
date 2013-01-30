using System.Collections.Generic;
using Contrib.Cache.Models;

namespace Contrib.Cache.ViewModels {
    public class StatisticsViewModel {
        public IEnumerable<CacheItem> CacheItems { get; set; }
        public dynamic Pager { get; set; }
    }
}