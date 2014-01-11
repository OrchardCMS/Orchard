using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.Templates {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ShapePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Name", c => c.WithLength(100))
                .Column<string>("Template", c => c.Unlimited()));

            ContentDefinitionManager.AlterPartDefinition("ShapePart", part => part
                .Attachable()
                .WithDescription("Turns a type into a shape provider."));

            ContentDefinitionManager.AlterTypeDefinition("Template", type => type
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .WithPart("ShapePart", p => p
                    .WithSetting("ShapePartSettings.Processor", "Razor"))
                .Draftable());
            return 1;
        }
    }
}