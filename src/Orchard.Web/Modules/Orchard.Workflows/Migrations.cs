using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Workflows {
    public class WorkflowsDataMigration : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("WorkflowPartRecord",
                table => table
                    .ContentPartRecord()
                );

            ContentDefinitionManager.AlterTypeDefinition("Workflow",
               cfg => cfg
                   .WithPart("WorkflowPart")
                   .WithPart("TitlePart")
                   .WithPart("IdentityPart")
                   .WithPart("CommonPart", p => p.WithSetting("OwnerEditorSettings.ShowOwnerEditor", "false"))
                );

            return 1;
        }
    }
}