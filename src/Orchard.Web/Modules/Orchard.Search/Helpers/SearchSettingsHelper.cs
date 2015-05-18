using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Search.Models;

namespace Orchard.Search.Helpers {
    public static class SearchSettingsHelper {
        public static IDictionary<string, string[]> DeserializeSearchFields(string value) {
            // Format: "<Index1:Field1,Field2>[|<Index1:Field1,Field2>]".
            // Example: "Search:title,body|Documents:filename,title".

            var data = value ?? "";
            var items = data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var dictionary = new Dictionary<string, string[]>();

            foreach (var item in items) {
                var pair = item.Split(new[] { ':' }, StringSplitOptions.None);
                var index = pair[0];
                var fields = pair[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                dictionary[index] = fields;
            }
            return dictionary;
        }

        public static string SerializeSearchFields(IDictionary<string, string[]> value) {
            var data = String.Join("|", value.Select(x => String.Format("{0}:{1}", x.Key, String.Join(",", x.Value))));
            return data;
        }

        public static string[] GetSearchFields(this AdminSearchSettingsPart part) {
            return part.SearchFields.ContainsKey(part.SearchIndex) ? part.SearchFields[part.SearchIndex] : new string[0];
        }

        public static string[] GetSearchFields(this SearchSettingsPart part) {
            return part.SearchFields.ContainsKey(part.SearchIndex) ? part.SearchFields[part.SearchIndex] : new string[0];
        }
    }
}