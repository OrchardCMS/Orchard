namespace Orchard.Roles.Records {
    public class PermissionRecord {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string ModuleName { get; set; }
        public virtual string Description { get; set; }
    }
}