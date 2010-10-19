using System;
using Orchard.Data.Migration;

namespace Orchard.Indexing {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("IndexingTaskRecord", 
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Action")
                    .Column<DateTime>("CreatedUtc")
                    .Column<int>("ContentItemRecord_id")
                );

            return 1;
        }
    }
}