using System.ServiceModel.Syndication;

namespace Orchard.Packaging {
    public class PackagingEntry {
        public PackagingSource Source { get; set; }
        public SyndicationFeed SyndicationFeed { get; set; }
        public SyndicationItem SyndicationItem { get; set; }
        public string PackageStreamUri { get; set; }
    }
}