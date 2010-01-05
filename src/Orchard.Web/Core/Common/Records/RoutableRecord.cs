using Orchard.ContentManagement.Records;

namespace Orchard.Core.Common.Records {
    public class RoutableRecord : ContentPartVersionRecord {
        public virtual string Title { get; set; }
        public virtual string Slug { get; set; }
    }
}
