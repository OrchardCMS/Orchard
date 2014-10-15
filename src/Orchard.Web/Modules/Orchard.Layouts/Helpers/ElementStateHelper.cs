using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Helpers {
    public static class ElementStateHelper {
        private static readonly string[] _elementStateBlackList = {"ElementState", "__RequestVerificationToken"};

        public static string Get(this StateDictionary state, string key, string defaultValue = null) {
            return state == null ? null : state.ContainsKey(key) ? state[key] : null;
        }

        public static string Serialize(this StateDictionary state) {
            return state == null ? "" : String.Join("&", state.Select(x => String.Format("{0}={1}", x.Key, HttpUtility.UrlEncode(x.Value))));
        }

        public static StateDictionary Combine(this StateDictionary target, StateDictionary input, bool removeNonExistingItems = false) {
            var combined = new StateDictionary(target);
            
            foreach (var item in input) {
                combined[item.Key] = item.Value;
            }

            if (removeNonExistingItems) {
                foreach (var item in target) {
                    if (!input.ContainsKey(item.Key)) {
                        combined.Remove(item.Key);
                    }
                }
            }
            return combined;
        }

        public static string Serialize(this NameValueCollection collection) {
            return collection.ToDictionary().Serialize();
        }

        public static StateDictionary Deserialize(string state) {
            var dictionary = new StateDictionary();
            if (String.IsNullOrWhiteSpace(state))
                return dictionary;

            var items = state.Split(new[] { '&' });

            foreach (var item in items) {
                var pair = item.Split(new[] { '=' });
                var key = pair[0];
                var value = HttpUtility.UrlDecode(pair[1]);

                if (!dictionary.ContainsKey(key) && !_elementStateBlackList.Contains(key))
                    dictionary.Add(key, value);
            }

            return dictionary;
        }

        public static StateDictionary ToDictionary(this NameValueCollection nameValues) {
            var copy = new NameValueCollection(nameValues);

            foreach (var key in _elementStateBlackList) {
                copy.Remove(key);
                
            }
            var dictionary = new StateDictionary();

            foreach (string key in copy) {
                dictionary[key] = copy[key];
            }

            return dictionary;
        }

        public static IDictionary<string, object> ToTokenDictionary(this NameValueCollection nameValues) {
            var copy = new NameValueCollection(nameValues);

            foreach (var key in _elementStateBlackList) {
                copy.Remove(key);

            }
            var dictionary = new Dictionary<string, object>();

            foreach (string key in copy) {
                dictionary[key] = copy[key];
            }

            return dictionary;
        }
    }
}