using System.Collections.Generic;

namespace Orchard.Layouts.Helpers {
    public static class DictionaryExtensions {
        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) {
            if (dictionary.ContainsKey(key)) {
                dictionary.Remove(key);
                return true;
            }
            return false;
        }

        public static void Combine<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> input) {
            foreach (var item in input) {
                target[item.Key] = item.Value;
            }
        }
    }
}