namespace Orchard.Roles.Models {
    public class PermissionRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string FeatureName { get; set; }
        public virtual string Description { get; set; }
    }
}