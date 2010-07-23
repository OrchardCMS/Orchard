using System;

namespace Orchard.Packaging.Services {
    public class PackagingSource {
        public Guid Id { get; set; }
        public string FeedTitle { get; set; }
        public string FeedUrl { get; set; }
    }
}