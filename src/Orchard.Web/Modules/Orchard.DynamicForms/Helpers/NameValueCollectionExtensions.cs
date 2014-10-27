using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Orchard.DynamicForms.Helpers {
    public static class NameValueCollectionExtensions {
        public static string ToQueryString(this NameValueCollection nameValues) {
            return String.Join("&", (from string name in nameValues select String.Concat(name, "=", HttpUtility.UrlEncode(nameValues[name]))).ToArray());
        }
    }
}