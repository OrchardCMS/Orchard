using System;
using Orchard.Data.Migration;

namespace Orchard.Tasks.Locking.Migrations {
    public class FrameworkMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("DistributedLockRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.NotNull().WithLength(256))
                .Column<string>("MachineName", column => column.WithLength(256))
                .Column<int>("ThreadId", column => column.Nullable())
                .Column<int>("Count")
                .Column<DateTime>("CreatedUtc")
                .Column<DateTime>("ValidUntilUtc"));

            return 1;
        }
    }
}