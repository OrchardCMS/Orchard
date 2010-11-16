using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Routable.Models {
    public class RoutePartRecord : ContentPartVersionRecord {
        [StringLength(1024)]
        public virtual string Title { get; set; }

        [StringLength(1024)]
        public virtual string Slug { get; set; }

        [StringLength(2048)]
        public virtual string Path { get; set; }
    }
}
