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
                return _contentItems.Value;
            }
        }

        private string EncodeIds(ICollection<int> ids) {
            if (ids == null || !ids.Any()) {
                return string.Empty;
            }

            // use {1},{2} format so it can be filtered with delimiters
            return "{" + string.Join("},{", ids.ToArray()) + "}";
        }

        private int[] DecodeIds(string ids) {
            if(String.IsNullOrWhiteSpace(ids)) {
                return new int[0];
            }

            return ids.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }
    }
}
