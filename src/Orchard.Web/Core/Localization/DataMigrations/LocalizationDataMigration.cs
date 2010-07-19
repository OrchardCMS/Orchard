using Orchard.Data.Migration;

namespace Orchard.Core.Localization.DataMigrations {
    public class LocalizationDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Localization_LocalizedRecord (Id INTEGER not null, CultureId INTEGER, MasterContentItemId INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("LocalizedRecord", table => table
                .ContentPartRecord()
                .Column<int>("CultureId")
                .Column<int>("MasterContentItemId")
                );

            return 1;
        }
    }
}