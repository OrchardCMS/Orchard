namespace Orchard.Roles.Models {
    public class RolesPermissions {
        public virtual int Id { get; set; }
        public virtual RoleRecord Role { get; set; }
        public virtual PermissionRecord Permission { get; set; }
    }
}
