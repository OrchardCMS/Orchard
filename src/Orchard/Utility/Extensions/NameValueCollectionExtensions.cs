using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Orchard.Utility.Extensions {
    public static class NameValueCollectionExtensions {
        public static string ToQueryString(this NameValueCollection nameValues) =>
            string.Join(
                "&",
                (from string name in nameValues select string.Concat(name, "=", HttpUtility.UrlEncode(nameValues[name]))).ToArray());
    }
}
