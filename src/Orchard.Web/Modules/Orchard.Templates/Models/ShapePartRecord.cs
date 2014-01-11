using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Templates.Models {
    public class ShapePartRecord : ContentPartRecord {
        public virtual string Name { get; set; }

        [StringLengthMax]
        public virtual string Template { get; set; }
    }
}