using Orchard.Data.Migration;

namespace Orchard.Search.DataMigrations {
    public class SearchDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("SearchSettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<bool>("FilterCulture")
                .Column<string>("SearchedFields")
                );

            return 0010;
        }
    }
}