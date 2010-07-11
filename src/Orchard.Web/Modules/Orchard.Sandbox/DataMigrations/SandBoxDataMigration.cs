using Orchard.Data.Migration;

namespace Orchard.Sandbox.DataMigrations {
    public class SandboxDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("SandboxPageRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("SandboxSettingsRecord", table => table
                .ContentPartRecord()
                .Column<bool>("AllowAnonymousEdits")
                );

            return 0010;
        }
    }
}