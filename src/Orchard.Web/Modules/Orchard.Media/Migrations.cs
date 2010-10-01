using Orchard.Data.Migration;

namespace Orchard.Media {
    public class MediaDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Media_MediaSettingsRecord (Id INTEGER not null, RootMediaFolder TEXT, primary key (Id));
            SchemaBuilder.CreateTable("MediaSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("RootMediaFolder")
                );

            return 1;
        }

        public int UpdateFrom1() {
            // Filters.Add(new ActivatingFilter<MediaSettingsPart>("Site"));

            return 2;
        }
    }
}