using Orchard.Data.Migration;
using Orchard.Roles.Services;

namespace Orchard.Roles {
    public class RolesDataMigration : DataMigrationImpl {
        private readonly IRoleService _roleService;

        public RolesDataMigration(IRoleService roleService) {
            _roleService = roleService;
        }

        public int Create() {
            SchemaBuilder.CreateTable("PermissionRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                    .Column<string>("FeatureName")
                    .Column<string>("Description")
                );

            SchemaBuilder.CreateTable("RoleRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("RolesPermissionsRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Role_id")
                    .Column<int>("Permission_id")
                    .Column<int>("RoleRecord_Id")
                );

            SchemaBuilder.CreateTable("UserRolesPartRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("UserId")
                    .Column<int>("Role_id")
                );

            return 2;
        }

        public int UpdateFrom1() {

            // creates default permissions for Orchard v1.4 instances and earlier
            _roleService.CreatePermissionForRole("Anonymous", Orchard.Core.Contents.Permissions.ViewContent.Name);
            _roleService.CreatePermissionForRole("Authenticated", Orchard.Core.Contents.Permissions.ViewContent.Name);

            return 2;
        }
    }
}