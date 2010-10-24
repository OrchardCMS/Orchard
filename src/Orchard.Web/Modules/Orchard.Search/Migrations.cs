using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Search {
    public class SearchDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("SearchSettingsPartRecord", table => table
                .ContentPartRecord()
                    .Column<bool>("FilterCulture")
                    .Column<string>("SearchedFields", c => c.Unlimited())
                );

            ContentDefinitionManager.AlterTypeDefinition("SearchForm",
                cfg => cfg
                    .WithPart("SearchFormPart")
                    .WithPart("CommonPart")
                    .WithPart("WidgetPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }
    }
}