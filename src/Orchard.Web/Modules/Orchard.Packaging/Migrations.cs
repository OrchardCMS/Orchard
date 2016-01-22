using Orchard.Data.Migration;

namespace Orchard.Packaging {
    public class Migrations: DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PackagingSource", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("FeedTitle", c => c.WithLength(255))
                    .Column<string>("FeedUrl", c => c.WithLength(2048))
                );

            return 1;
        }
    }
}