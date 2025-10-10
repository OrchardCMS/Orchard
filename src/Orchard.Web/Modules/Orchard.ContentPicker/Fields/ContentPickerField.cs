using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentPicker.Fields {
    public class ContentPickerField : ContentField {
        private static readonly char[] separator = new [] {'{', '}', ','};
        internal LazyField<IEnumerable<ContentItem>> _contentItems = new LazyField<IEnumerable<ContentItem>>();
 
        public int[] Ids {
            get { return DecodeIds(Storage.Get<string>()); }
            set { Storage.Set(EncodeIds(value)); }
        }

        public IEnumerable<ContentItem> ContentItems { 
            get {
                return _contentItems.Value ?? Enumerable.Empty<ContentItem>();
            }
        }

        private static string EncodeIds(ICollection<int> ids) {
            if (ids == null || !ids.Any()) {
                return string.Empty;
            }

            // use {1},{2} format so it can be filtered with delimiters
            return "{" + string.Join("},{", ids.ToArray()) + "}";
        }

        public static int[] DecodeIds(string ids) {
            if(String.IsNullOrWhiteSpace(ids)) {
                return new int[0];
            }
            // if some of the slices of the string cannot be properly parsed,
            // we still will return those that can.
            return ids
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    int i = -1;
                    if(int.TryParse(s, out i)) {
                        return i;
                    }
                    // if we can't parse return a negative value
                    return -1;
                })
                // take only those that parsed properly
                .Where(i => i > 0)
                .ToArray();
        }
    }
}
