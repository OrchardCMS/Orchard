using System;
using Orchard.Data.Migration;

namespace Orchard.JobsQueue {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("QueuedJobRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Message", c => c.WithLength(64))
                .Column<string>("Parameters", c => c.Unlimited())
                .Column<int>("Priority", c => c.WithDefault(0))
                .Column<DateTime>("CreatedUtc")
                );

            return 1;
        }
    }
}