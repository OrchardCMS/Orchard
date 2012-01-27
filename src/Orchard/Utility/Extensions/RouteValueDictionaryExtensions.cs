using System;
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

        public static bool Compare(this RouteValueDictionary x, RouteValueDictionary y) {
            if(x == y) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            if(x.Count != y.Count) {
                return false;
            }

            // keys can be different in case
            return x.Keys.All(key => x[key].ToString().Equals(y[key].ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}