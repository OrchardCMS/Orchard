using Orchard.Data.Migration;

namespace Orchard.Translations {
    public class RolesDataMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TranslatableRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("Location")
                    .Column<string>("Value")
                );

            SchemaBuilder.CreateTable("TranslatedRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Parent_Id")
                    .Column<int>("Culture_Id")
                    .Column<string>("Value")
                );

            return 1;
        }
    }
}