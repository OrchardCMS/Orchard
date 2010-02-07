using System;

namespace Orchard.Pages.Extensions {
    //TODO: Move to orchard.dll
    public static class UriExtensions {
        public static string ToRootString(this Uri uri) {
            return string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, uri.Port != 80 ? ":" + uri.Port : "");
        }
    }
}