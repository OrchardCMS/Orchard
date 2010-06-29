using System;
using Orchard.Data.Migration;

namespace Orchard.Indexing.DataMigrations {
    public class IndexingDataMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("IndexingTaskRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey())
                .Column<int>("Action")
                .Column<DateTime>("CreatedUtc")
                .Column<int>("ContentItemRecord_id")
                );

            return 0010;
        }
    }
}