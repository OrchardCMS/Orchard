using Orchard.Data.Migration;

namespace Orchard.Core.Routable.DataMigrations {
    public class RoutableDataMigration : DataMigrationImpl {

        public int Create() {
            //CREATE TABLE Routable_RoutableRecord (Id INTEGER not null, Title TEXT, Slug TEXT, Path TEXT, ContentItemRecord_id INTEGER, primary key (Id));
            SchemaBuilder.CreateTable("RoutableRecord", table => table
                .ContentPartVersionRecord()
                .Column<string>("Title", column => column.WithLength(1024))
                .Column<string>("Slug")
                .Column<string>("Path", column => column.WithLength(2048))
                );

            return 0010;
        }
    }
}