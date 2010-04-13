using System;

namespace Orchard.Utility.Extensions {
    public static class UriExtensions {
        public static string ToRootString(this Uri uri) {
            return string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.Port != 80 ? ":" + uri.Port : "");
        }
    }
}