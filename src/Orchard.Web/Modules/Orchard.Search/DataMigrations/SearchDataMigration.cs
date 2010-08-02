using Orchard.Data.Migration;

namespace Orchard.Search.DataMigrations {
    public class SearchDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("SearchSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("FilterCulture")
                .Column<string>("SearchedFields")
                );

            return 1;
        }
    }
}