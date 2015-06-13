using Orchard.Data.Conventions;

namespace IDeliverable.ThemeSettings.Models
{
    public class ThemeProfileRecord
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        [StringLengthMax]
        public virtual string Description { get; set; }
        public virtual string Theme { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }
        public virtual bool IsCurrent { get; set; }
    }
}