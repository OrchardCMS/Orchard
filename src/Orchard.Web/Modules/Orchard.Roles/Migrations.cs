using Orchard.Data.Migration;

namespace Orchard.Roles {
    public class RolesDataMigration : DataMigrationImpl {

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

            return 1;
        }
    }
}