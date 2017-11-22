using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.MediaLibrary.Models;
using Orchard.ContentManagement.Utilities;

namespace Orchard.MediaLibrary.Fields {
    public class MediaLibraryPickerField : ContentField {
        private static readonly char[] separator = {'{', '}', ','};
        internal LazyField<IEnumerable<MediaPart>> _contentItems = new LazyField<IEnumerable<MediaPart>>();
 
        public int[] Ids {
            get { return DecodeIds(Storage.Get<string>()); }
            set { Storage.Set(EncodeIds(value)); }
        }

        public IEnumerable<MediaPart> MediaParts { 
            get {
                return _contentItems != null ? _contentItems.Value : Enumerable.Empty<MediaPart>();
            }
            set { _contentItems.Value = value; }
        }

        /// <summary>
        /// Gets the MediaUrl property of the first Media, or null if none
        /// </summary>
        public string FirstMediaUrl {
            get {
                if (!MediaParts.Any()) {
                    return null;
                }

                return MediaParts.First().MediaUrl;
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
