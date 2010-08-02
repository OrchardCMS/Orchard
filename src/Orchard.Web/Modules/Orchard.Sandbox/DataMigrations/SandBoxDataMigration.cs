using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Sandbox.DataMigrations {
    public class SandboxDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("SandboxPagePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name")
                );

            SchemaBuilder.CreateTable("SandboxSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("AllowAnonymousEdits")
                );

            return 1;
        }

        public int UpdateFrom1() {

            ContentDefinitionManager.AlterTypeDefinition("SandboxPage", 
                cfg => cfg
                    .WithPart("SandboxPagePart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("BodyPart")
                );
            
            return 2;
        }
    }
}