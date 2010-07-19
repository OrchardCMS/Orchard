using Orchard.Data.Migration;

namespace Orchard.Media.DataMigrations {
    public class MediaDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Orchard_Media_MediaSettingsRecord (Id INTEGER not null, RootMediaFolder TEXT, primary key (Id));
            SchemaBuilder.CreateTable("MediaSettingsRecord", table => table
                .ContentPartRecord()
                .Column<string>("RootMediaFolder")
                );

            return 1;
        }
    }
}