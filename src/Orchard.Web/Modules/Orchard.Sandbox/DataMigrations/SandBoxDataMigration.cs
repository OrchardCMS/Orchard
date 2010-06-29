using Orchard.Data.Migration;

namespace Orchard.Sandbox.DataMigrations {
    public class SandboxDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("SandboxPageRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("SandboxSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<bool>("AllowAnonymousEdits")
                );

            return 0010;
        }
    }
}