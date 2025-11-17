using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Search.Models;

namespace Orchard.Search.Helpers
{
    public static class SearchSettingsHelper
    {
        public static IDictionary<string, string[]> DeserializeSearchFields(string value)
        {
            // Format: "<Index1:Field1,Field2>[|<Index1:Field1,Field2>]".
            // Example: "Search:title,body|Documents:filename,title".

            var data = value ?? "";
            var items = data.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var dictionary = new Dictionary<string, string[]>();

            foreach (var item in items)
            {
                var pair = item.Split(new[] { ':' }, StringSplitOptions.None);
                var index = pair[0];
                var fields = pair[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                dictionary[index] = fields;
            }
            return dictionary;
        }

        public static string SerializeSearchFields(IDictionary<string, string[]> value)
        {
            var data = string.Join("|", value.Select(x => string.Format("{0}:{1}", x.Key, string.Join(",", x.Value))));
            return data;
        }

        public static string[] GetSearchFields(this SearchSettingsPart part, string index)
        {
            return part.SearchFields.ContainsKey(index) ? part.SearchFields[index] : new string[0];
        }

        /// <summary>
        /// Merging the existing search indexes with their search fields by keeping the existing search indexes
        /// and their search fields, but replacing the search fields of the existing search indexes.
        /// </summary>
        /// <param name="existingSearchFields">The existing search indexes with their search fields.</param>
        /// <param name="searchFieldsToAdd">The new search indexes with their search fields to import.</param>
        /// <returns>The merged search indexes with their search fields.</returns>
        public static string MergeSearchFields(IDictionary<string, string[]> existingSearchFields, IDictionary<string, string[]> searchFieldsToAdd)
        {
            var mergedSearchFields = new Dictionary<string, string[]>();

            foreach (var newSearchFieldsToAdd in searchFieldsToAdd.Keys)
            {
                if (!mergedSearchFields.ContainsKey(newSearchFieldsToAdd))
                {
                    mergedSearchFields.Add(newSearchFieldsToAdd, searchFieldsToAdd[newSearchFieldsToAdd]);
                }
            }

            foreach (var existingSearchFieldsToAdd in existingSearchFields.Keys)
            {
                if (!mergedSearchFields.ContainsKey(existingSearchFieldsToAdd))
                {
                    mergedSearchFields.Add(existingSearchFieldsToAdd, existingSearchFields[existingSearchFieldsToAdd]);
                }
            }

            return SerializeSearchFields(mergedSearchFields);
        }
    }
}
