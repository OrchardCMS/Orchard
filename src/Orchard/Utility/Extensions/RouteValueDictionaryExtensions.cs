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
    }
}