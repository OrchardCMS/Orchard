namespace Orchard.Roles.Models {
    public class RolesPermissionsRecord {
        public virtual int Id { get; set; }
        public virtual RoleRecord Role { get; set; }
        public virtual PermissionRecord Permission { get; set; }
    }
}