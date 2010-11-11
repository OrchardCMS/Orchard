using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Orchard.Packaging {
    [OrchardFeature("PackagingServices")]
    public class Migrations: DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PackagingSourceRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("FeedTitle", c => c.WithLength(255))
                    .Column<string>("FeedUrl", c => c.WithLength(2048))
                );

            return 1;
        }
    }
}