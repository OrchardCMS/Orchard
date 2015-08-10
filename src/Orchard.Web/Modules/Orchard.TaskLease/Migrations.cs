using System;
using Orchard.Data.Migration;

namespace Orchard.TaskLease {
    public class TaskLeaseMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("TaskLeaseRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("TaskName")
                    .Column<string>("MachineName")
                    .Column<DateTime>("UpdatedUtc")
                    .Column<DateTime>("ExpiredUtc")
                    .Column<string>("State", c => c.Unlimited())
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("DatabaseLockRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Name", column => column.NotNull().WithLength(256))
                .Column<DateTime>("AcquiredUtc"));

            return 2;
        }
    }
}