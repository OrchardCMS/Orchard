using System;

namespace Orchard.MediaLibrary.Models { 
    public class MediaFolder : IMediaFolder {
        public string Name { get; set; }
        public string MediaPath { get; set; }
        public string User { get; set; }
        public DateTime LastUpdated { get; set; }

        private Lazy<long> _size;
        internal Lazy<long> SizeField {
            get { return _size; }
            set { _size = value; }
        }

        public long Size {
            get { return _size.Value; }
        }

        public static string[] InvalidNameCharacters = { "/", "\\", "<", ">", "*", "%", "&", ":", "?" };
        // These pattern was generated offline using the array above and encoded for
        // use in razor pages
        public static string InvalidNameCharactersPattern = @"[^/\\<>*%&:\\?]+"; 
    }
}
