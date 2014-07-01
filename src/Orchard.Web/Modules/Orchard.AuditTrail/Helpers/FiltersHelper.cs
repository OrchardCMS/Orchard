using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.Helpers {
    public static class FiltersHelper {
        public static string Get(this Filters filters, string key) {
            if (!filters.ContainsKey(key))
                return null;

            return filters[key];
        }
    }
}