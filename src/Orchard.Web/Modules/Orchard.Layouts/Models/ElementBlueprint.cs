using Orchard.Data.Conventions;

namespace Orchard.Layouts.Models {
    public class ElementBlueprint {
        public virtual int Id { get; set; }
        public virtual string BaseElementTypeName { get; set; }
        public virtual string ElementTypeName { get; set; }
        public virtual string ElementDisplayName { get; set; }
        public virtual string ElementDescription { get; set; }
        public virtual string ElementCategory { get; set; }

        [StringLengthMax]
        public virtual string BaseElementState { get; set; }
    }
}