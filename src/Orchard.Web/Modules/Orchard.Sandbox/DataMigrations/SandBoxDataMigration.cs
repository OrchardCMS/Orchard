using Orchard.ContentManagement.MetaData;
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

            return 1;
        }

        public int UpdateFrom1() {

            ContentDefinitionManager.AlterTypeDefinition("SandboxPage", 
                cfg => cfg
                    .WithPart("SandboxPage")
                    .WithPart("CommonAspect")
                    .WithPart("IsRoutable")
                    .WithPart("BodyAspect")
                );
            
            return 2;
        }
    }
}