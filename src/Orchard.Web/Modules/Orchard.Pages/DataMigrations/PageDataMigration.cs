using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Pages.DataMigrations {
    public class PageDataMigration : DataMigrationImpl {

        public int Create() {

            ContentDefinitionManager.AlterTypeDefinition("Page",
              cfg => cfg
                  .WithPart("Page")
                  .WithPart("CommonPart")
                  .WithPart("RoutePart")
                  .WithPart("BodyPart")
               );
            
            return 1;
        }


        public int UpdateFrom1() {
            ContentDefinitionManager.AlterTypeDefinition("Page",
                cfg => cfg
                    .WithSetting("ContentTypeSettings.Creatable", "true")
            );

            return 2;
        }
    }
}