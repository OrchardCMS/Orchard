using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace IDeliverable.Slides.Helpers
{
    public static class UrlStringExtensions
    {
        public static string AppendQueryString(this string url, object queryStringValues)
        {
            return AppendQueryString(url, new RouteValueDictionary(queryStringValues));
        }

        public static string AppendQueryString(this string url, IDictionary<string, object> queryStringValues)
        {
            var queryOperand = url.Contains("?") ? "&" : "?";
            var queryString = String.Join("&", queryStringValues.Select(x => x.Key + "=" + x.Value));
            return String.Concat(url, queryOperand, queryString);
        }
    }
}