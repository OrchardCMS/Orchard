using System.ServiceModel.Syndication;

namespace Futures.Modules.Packaging.Services {
    public class PackageEntry {
        public PackageSource Source { get; set; }
        public SyndicationFeed SyndicationFeed { get; set; }
        public SyndicationItem SyndicationItem { get; set; }
        public string PackageStreamUri { get; set; }
    }
}