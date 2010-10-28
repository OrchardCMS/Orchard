using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Orchard.ContentQueries {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            //SchemaBuilder.CreateTable("ContentQueryPartRecord",
            //    table => table
            //        .ContentPartRecord()
            //    );

            ContentDefinitionManager.AlterTypeDefinition("ContentQuery",
                cfg => cfg
                    .WithPart("ContentQueryPart")
                    .WithPart("CommonPart")
                    .WithPart("RoutePart")
                    .WithPart("MenuPart")
                    .Creatable()
                );

            ContentDefinitionManager.AlterTypeDefinition("ContentQueryWidget",
                cfg => cfg
                    .WithPart("ContentQueryPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }
    }
}