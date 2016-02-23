using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.MediaProcessing {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ImageProfilePartRecord",
                                      table => table
                                                   .Column<string>("Name", c => c.WithLength(255))
                                                   .ContentPartRecord()
                );

            ContentDefinitionManager.AlterTypeDefinition("ImageProfile",
                                                         cfg => cfg
                                                                    .WithPart("ImageProfilePart")
                                                                    .WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                                                                    .WithIdentity()
                );

            SchemaBuilder.CreateTable("FilterRecord",
                                      table => table
                                                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                                                   .Column<string>("Category", c => c.WithLength(64))
                                                   .Column<string>("Type", c => c.WithLength(64))
                                                   .Column<string>("Description", c => c.WithLength(255))
                                                   .Column<string>("State", c => c.Unlimited())
                                                   .Column<int>("Position")
                                                   .Column<int>("ImageProfilePartRecord_id")
                );

            SchemaBuilder.CreateTable("FileNameRecord",
                                      table => table
                                                   .Column<int>("Id", c => c.PrimaryKey().Identity())
                                                   // Note: path and file name set to unlimited length, should have a sensible length
                                                   .Column<string>("Path", c => c.Unlimited())
                                                   .Column<string>("FileName", c => c.Unlimited())
                                                   .Column<int>("ImageProfilePartRecord_id")
                );

            return 1;
        }
    }
}