using System;
using Orchard.Data.Migration;

namespace Orchard.Messaging {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("QueuedMessageRecord", table => table
                .Column<int>("Id", c => c.Identity().PrimaryKey())
                .Column<string>("Type", c => c.WithLength(64))
                .Column<int>("Priority", c => c.WithDefault(0))
                .Column<string>("Payload", c => c.Unlimited())
                .Column<string>("Status", c => c.WithLength(64))
                .Column<string>("Result", c => c.Unlimited())
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("StartedUtc")
                .Column<DateTime>("CompletedUtc")
                );

            return 1;
        }
    }
}