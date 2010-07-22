using Orchard.Data.Migration;

namespace Orchard.Roles.DataMigrations {
    public class RolesDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Roles_PermissionRecord (Id  integer, Name TEXT, ModuleName TEXT, Description TEXT, primary key (Id));
            SchemaBuilder.CreateTable("PermissionRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                .Column<string>("ModuleName")
                .Column<string>("Description")
                );

            //CREATE TABLE Orchard_Roles_RoleRecord (Id  integer, Name TEXT, primary key (Id));
            SchemaBuilder.CreateTable("RoleRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name")
                );

            //CREATE TABLE Orchard_Roles_RolesPermissionsRecord (Id  integer, Role_id INTEGER, Permission_id INTEGER, RoleRecord_Id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("RolesPermissionsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("Role_id")
                .Column<int>("Permission_id")
                .Column<int>("RoleRecord_Id")
                );

            //CREATE TABLE Orchard_Roles_UserRolesRecord (Id  integer, UserId INTEGER, Role_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("UserRolesPartRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("UserId")
                .Column<int>("Role_id")
                );

            return 1;
        }
    }
}