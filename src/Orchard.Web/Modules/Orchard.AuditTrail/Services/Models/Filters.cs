using System.Collections.Generic;
using System.Collections.Specialized;

namespace Orchard.AuditTrail.Services.Models {
    public class Filters : Dictionary<string, string> {
        public static Filters From(NameValueCollection nameValues) {
            var filters = new Filters();

            foreach (string nameValue in nameValues) {
                filters.Add(nameValue, nameValues[nameValue]);
            }

            return filters;
        }

        public Filters AddFilter(string key, string value) {
            Add(key, value);
            return this;
        }
    }
}