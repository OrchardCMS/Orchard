using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Routing;

namespace Orchard.Utility.Extensions {
    public static class RouteValueDictionaryExtensions {
        public static RouteValueDictionary Merge(this RouteValueDictionary dictionary, object values) {
            return values == null ? dictionary : dictionary.Merge(new RouteValueDictionary(values));
        }

        public static RouteValueDictionary Merge(this RouteValueDictionary dictionary, RouteValueDictionary dictionaryToMerge) {
            if (dictionaryToMerge == null)
                return dictionary;

            var newDictionary = new RouteValueDictionary(dictionary);

            foreach (var valueDictionary in dictionaryToMerge)
                newDictionary[valueDictionary.Key] = valueDictionary.Value;

            return newDictionary;
        }

        public static bool Match(this RouteValueDictionary x, RouteValueDictionary y) {
            if(x == y) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            if(x.Count != y.Count) {
                return false;
            }

            var bools = x.Join(y, 
                kv1 => kv1.Key.ToLowerInvariant(), 
                kv2 => kv2.Key.ToLowerInvariant(), 
                (kv1, kv2) => StringMatch(kv1.Value, kv2.Value)
                ).ToArray();

            return bools.All(b => b) && bools.Count() == x.Count;
        }

        private static bool StringMatch(object value1, object value2) {
            return string.Equals(
                Convert.ToString(value1, CultureInfo.InvariantCulture),
                Convert.ToString(value2, CultureInfo.InvariantCulture),
                StringComparison.InvariantCultureIgnoreCase
                );
        }

        public static RouteValueDictionary ToRouteValueDictionary(this IEnumerable<KeyValuePair<string, string>> routeValues) {
            if (routeValues == null)
                return null;

            var result = new RouteValueDictionary();
            foreach (var routeValue in routeValues) {
                if (routeValue.Key.EndsWith("-")) {
                    result.Add(routeValue.Key.Substring(0, routeValue.Key.Length - 1), routeValue.Value);
                }
                else {
                    result.Add(routeValue.Key, routeValue.Value);
                }
            }
            return result;
        }
    }
}