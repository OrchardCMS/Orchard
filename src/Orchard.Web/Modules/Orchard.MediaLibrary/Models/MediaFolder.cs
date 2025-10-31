using System;

namespace Orchard.MediaLibrary.Models
{
    public class MediaFolder : IMediaFolder
    {
        public string Name { get; set; }
        public string MediaPath { get; set; }
        public string User { get; set; }
        public DateTime LastUpdated { get; set; }
        internal Lazy<long> SizeField { get; set; }

        public long Size => SizeField.Value;
    }
}
