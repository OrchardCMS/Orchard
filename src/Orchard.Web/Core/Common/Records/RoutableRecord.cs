using Orchard.Models.Records;

namespace Orchard.Core.Common.Records {
    public class RoutableRecord : ModelPartRecord {
        public virtual string Title { get; set; }
        public virtual string Slug { get; set; }
    }
}
